program adventure_game;
begin
    {* Global game state *}
    var name : string;
    var age, random_num : integer;
    var hitpoints, maxhitpoints, turn: integer;
    maxhitpoints := 60;
    hitpoints := maxhitpoints;
    turn := 1;
       
    var width : integer;
    var height : integer;
    
    width := 15;
    height := 15;    

    var commands : array [9] of string;
    commands[0] := "1";
    
    {* Utility functions *}
    
    {* Linear congruential generator 
       Returns number between 0 and max-1 *}
    function random(max : integer) : integer;
    begin
        random_num := (214013 *random_num + 2531011 ) % 65535;
        
        if random_num < 0 then
            random_num := -random_num;        
        
        return random_num % max;        
    end;
    
    procedure cls();
    begin
        {* 
        Requires either Windows 10 or posix-compliant shell! 
        Note: cmd.exe seems to work out of the box, powershell seems to require a script listed here:
        http://www.nivot.org/blog/post/2016/02/04/Windows-10-TH2-%28v1511%29-Console-Host-Enhancements
        *}
        writeln("\\e[1;1H\\e[2J");   
    end;


    {* Converts x/y coordinate into a form that's usable with one dimensional array *}
    function get_coordinate(x : integer, y : integer, width : integer) : integer;
    begin
        return y*width + x;        
    end;
    
    
    {* Game functions *} 
    
    {* Uses reference instead of global state just because I can *}
    procedure initialize_map(var passability : array [] of boolean, width : integer, height : integer);
    begin 
        var wallChange, x, y : integer;
        wallChange := 30;
        x := 0;
        y := 0;
               
        while y < height do
        begin
            while x < width do
            begin
                if random(100) < wallChange then
                    passability[get_coordinate(x, y, width)] := false
                else
                    passability[get_coordinate(x, y, width)] := true;
                x := x + 1;         
            end;          
            y :=y + 1;  
            x := 0;
        end;       
    end;
    
    procedure print_hud();
    begin
        writeln("Name: ", name, " Age: ", age, " HP: ", hitpoints, "/", maxhitpoints, " Turn: ", turn); 
        writeln();
    end;
    
    
    procedure print_map(
        passability : array [] of boolean, 
        player_x : integer, 
        player_y : integer, 
        width : integer, 
        height : integer);
    begin
        {* lots of copying\allocations going on here :/ *}
        var line : string;
        var x, y : integer;
        line := "";
        y := 0;
        while y < height do
        begin
            x := 0;
            while x < width do
            begin

                if (x = player_x) and (y = player_y) then
                    line := line + "@"
                else
                begin
                    if passability[get_coordinate(x, y, width)] then
                        line := line + " "
                    else
                        line := line + "#";
                end;

                x := x + 1;
    
            end;
            writeln(line);
            line := "";
            y := y + 1;
        end;                
    end;

    function is_valid_command(command : string) : boolean;
    begin
        var i : integer;
        i := 0;
        while i < commands.size do
        begin          
            if commands[i] = command then
                return true;
            i := i + 1;
        end;
        return false;
    end;


    function read_command() : string;
    begin
        var command : string;

        while true do
        begin
            writeln("Input command: (numpad 123456789 for direction, exit to quit)");
            read(command);
            if is_valid_command(command) then
                return command;

            writeln("Invalid command '", command, "'"); 

        end;
    end;


    procedure update_game_state(command : string);
    begin
        assert(is_valid_command(command));
        return;
    end;


    
    
   
    {* width x height grid as one dimensional array *}
    var passability : array [width*height] of boolean;
    var player_x, player_y : integer;
        
    player_x := 0;
    player_y := 0;
    {* Game start point *}
    writeln("What is your name?");
    read(name);
    
    writeln("How old are you?");
    read(age); 
    {* Seed random number generator with age, as we do not have access to more sophisticated source of enthropy *}
    random_num := age;
    initialize_map(passability, width, height);
    passability[get_coordinate(player_x, player_y, width)] := false; {* player position *}

    procedure main_2();
    begin
        var command : string;
        while true do
        begin

            cls();
            print_hud();
            print_map(passability, player_x, player_y, width, height);

            command := read_command();

            if command = "exit" then
                return;

            update_game_state("foo");

        end;
    end;  
end.