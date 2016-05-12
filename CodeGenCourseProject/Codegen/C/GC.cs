using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.Codegen.C
{
    internal class GC
    {

        internal const string C_OBJ_TYPE_NONE = "TYPE_NONE";
        internal const string C_OBJ_TYPE_STRING_ARRAY = "TYPE_STRING_ARRAY";
        /*
         * Naive mark-and-sweep garbage collector in C
         * Conceptually inspired by Boehm GC, although the code was written from scratch
         * https://en.wikipedia.org/wiki/Boehm_garbage_collector
         * 
         * Pointers are stored in a double-linked list with some additional meta-data
         * 
         * When heap is filled, the algorithm scans stack and registers and compares
         * the values to the stored pointers in the linked list. If the values match,
         * the pointer is considered to be in use. In case the object is a string array,
         * the pointers to strings are also considered to be in use.
         * 
         * After the scan is completed, the linked list is iterated over and any
         * objects not in use are freed.
         * 
         * Important note: This is highly compiler- and architecture specific implementation.
         * It uses GCC compiler intrinsics (__builtin_frame_address, asm()) and assumes
         * X86-64 architecture when scanning the registers.
         * 
         * Behaviour can be modified at compile time with following options:
         * 
         * -DGC_DISABLE: GC is disabled. It should compile on non-x86-64, non-gcc 
         *               architectures/compilers, although memory will leak.
         *               
         * -DMAX_HEAP_SIZE=XXX: Maximum heap size in bytes
         * 
         * -DGC_DEBUG: Enables debug prints
         * 
         * */
        static internal string GetGCCode()
        {
            return @"
enum Type { " + C_OBJ_TYPE_NONE + ", " + C_OBJ_TYPE_STRING_ARRAY + @"};

#ifndef GC_DISABLE

#ifndef MAX_HEAP_SIZE 
// 8MB heap by default
#define MAX_HEAP_SIZE 1024*1024*8
#endif

#ifdef GC_DEBUG
#define GC_PRINT(args...) printf(args)
#else
#define GC_PRINT(...)
#endif

typedef struct allocation {
    void *ptr;
    size_t allocation_size;
    struct allocation *next;
    struct allocation *prev;
    char mark;
    enum Type type;
} allocation_t;

typedef struct {
    size_t allocated;
    void *stack_sentinel;
    allocation_t *head;
} GC_State;

GC_State gc_state;
/* Add allocation to the allocations linked list */
void add_allocation(void *ptr, size_t size, enum Type type)
{
    GC_PRINT(""add_allocation(% p, % d)\n"", ptr, size);
    allocation_t * new_alloc = malloc(sizeof(allocation_t));
    new_alloc->ptr = ptr;
    new_alloc->allocation_size = size;
    new_alloc->mark = 0;
    new_alloc->type = type;

    new_alloc->prev = NULL;
    if (gc_state.head == NULL)
    {
        new_alloc->next = NULL;
        gc_state.head = new_alloc;
    }
    else
    {
        new_alloc->next = gc_state.head;
        gc_state.head->prev = new_alloc;
        gc_state.head = new_alloc;
    }

    gc_state.allocated += size;
}

void free_allocation(allocation_t* allocation)
{
    GC_PRINT(""remove_allocation(%p)\n"", allocation->ptr);

    if (allocation->next != NULL)
    {
        allocation->next->prev = allocation->prev;
    }
    if (allocation->prev != NULL)
    {
        allocation->prev->next = allocation->next;
    }

    if (allocation == gc_state.head)
    {
        gc_state.head = allocation->next;
    }
    gc_state.allocated -= allocation->allocation_size;
    free(allocation->ptr);
    free(allocation);
}


void gc_get_data(int* num_allocs, int* bytes_allocs)
{
    if (gc_state.head == NULL)
    {
        *num_allocs = 0;
        *bytes_allocs = 0;
        return;
    }

    *num_allocs = 1;
    allocation_t* cur = gc_state.head;
    *bytes_allocs = gc_state.head->allocation_size;

    while (cur->next != NULL)
    {
        cur = cur->next;
        *num_allocs += 1;
        *bytes_allocs += cur->allocation_size;
    }
}

void gc_init(void* stack_sentinel)
{
    GC_PRINT(""Initializing GC\n"");
    GC_PRINT(""MAX_HEAP_SIZE: %d\n"", MAX_HEAP_SIZE);
    gc_state.allocated = 0;
    gc_state.stack_sentinel = stack_sentinel;
    gc_state.head = NULL;
}

void gc_scan_stack();
void gc_scan_registers();
void gc_scan_graph_from(const allocation_t*);
void gc_sweep();

void gc_collect()
{
    GC_PRINT(""Collecting dead objects\n"");
    GC_PRINT(""Memory in use: %d bytes\n"", gc_state.allocated);
    if (gc_state.head == NULL)
    {
        GC_PRINT(""No allocated objects - stopping\n"");
        return;
    }
    gc_scan_stack();
    gc_scan_registers();
    gc_sweep();
    GC_PRINT(""GC finished\n"");
    GC_PRINT(""Memory in use: %d bytes\n"", gc_state.allocated);
}

void gc_scan_stack()
{
    GC_PRINT(""Scanning stack\n"");
    void* pos = __builtin_frame_address(0);
    while (pos <= gc_state.stack_sentinel)
    {
        void* as_ptr = *((void**)pos);
        allocation_t* alloc = gc_state.head;
        while (alloc != NULL)
        {
            if (alloc->ptr == as_ptr)
            {
                gc_scan_graph_from(alloc);
                alloc->mark = 1;
            }
            alloc = alloc->next;
        }

        // I'm not sure if GCC guarantees that stack is aligned.
        // if it is, we could use sizeof(void *) here instead.
        // I'm taking conservative stance to ensure correctness
        pos += 1;
    }
}

void gc_scan_registers()
{
    GC_PRINT(""Scanning registers\n"");
    long long registers[16];
    asm(""\t movq %%rax,%0"" : ""=r""(registers[0]));
    asm(""\t movq %%rbx,%0"" : ""=r""(registers[1]));
    asm(""\t movq %%rcx,%0"" : ""=r""(registers[2]));
    asm(""\t movq %%rdx,%0"" : ""=r""(registers[3]));
    asm(""\t movq %%rsi,%0"" : ""=r""(registers[4]));
    asm(""\t movq %%rdi,%0"" : ""=r""(registers[5]));
    asm(""\t movq %%rbp,%0"" : ""=r""(registers[6]));
    asm(""\t movq %%rsp,%0"" : ""=r""(registers[7]));
    asm(""\t movq %%r8,%0"" : ""=r""(registers[8]));
    asm(""\t movq %%r9,%0"" : ""=r""(registers[9]));
    asm(""\t movq %%r10,%0"" : ""=r""(registers[10]));
    asm(""\t movq %%r11,%0"" : ""=r""(registers[11]));
    asm(""\t movq %%r12,%0"" : ""=r""(registers[12]));
    asm(""\t movq %%r13,%0"" : ""=r""(registers[13]));
    asm(""\t movq %%r14,%0"" : ""=r""(registers[14]));
    asm(""\t movq %%r15,%0"" : ""=r""(registers[15]));

    int i = 0;
    for (; i < 16; ++i)
    {
        allocation_t* alloc = gc_state.head;
        while (alloc != NULL)
        {
            if (alloc->ptr == (void*)registers[i])
            {
                gc_scan_graph_from(alloc);
                alloc->mark = 1;
            }
            alloc = alloc->next;
        }
    }
}

void gc_scan_graph_from(const allocation_t* cur)
{
    if (cur->mark == 1)
    {
        GC_PRINT(""Object already marked - not scanning\n"");
        return;
    }
    
    if (cur->type == TYPE_STRING_ARRAY)
    {
        GC_PRINT(""String array - scanning\n"");
        string* arr = cur->ptr;
        size_t size = cur->allocation_size / sizeof(string*);
        int i = 0;
        for (; i<size; ++i)
        {
            allocation_t* alloc = gc_state.head;
            while (alloc != NULL)
            {
                if (alloc->ptr == arr[i])
                {
                    alloc->mark = 1;
                }
                alloc = alloc->next;
            }
        }
    }
}

void gc_sweep()
{
    allocation_t* alloc = gc_state.head;
    while (alloc != NULL)
    {
        allocation_t* cur = alloc;
        alloc = cur->next;
        if (cur->mark == 0)
        {
            free_allocation(cur);
        }
        else
        {
            cur->mark = 0;
        }
    }
}
#endif
void* gc_malloc(size_t size, enum Type type)
{
    #ifndef GC_DISABLE
    if (gc_state.allocated + size > MAX_HEAP_SIZE)
    {
        gc_collect();
        if (gc_state.allocated +  size > MAX_HEAP_SIZE)
        {
            fprintf(stderr, ""Out of memory\n"");
            exit(1);
        }
    }
    #endif
    void* ptr = malloc(size);

    # ifndef GC_DISABLE
    add_allocation(ptr, size, type);
    #endif
    return ptr;
}

void* gc_calloc(size_t num, size_t size, enum Type type)
{
    #ifndef GC_DISABLE
    if (gc_state.allocated + num* size > MAX_HEAP_SIZE)
    {
        gc_collect();
        if (gc_state.allocated + num* size > MAX_HEAP_SIZE)
        {
            fprintf(stderr, ""Out of memory\n"");
            exit(1);
        }
    }
    #endif
    void* ptr = calloc(num, size);
    #ifndef GC_DISABLE
    add_allocation(ptr, num* size, type);
    #endif
    return ptr;
}";
        }
    }
}
