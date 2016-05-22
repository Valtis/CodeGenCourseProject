program adventure_game;
begin
    {* Global game state *}
    var name : string;
    var age : integer;
    var hitpoints, maxhitpoints, turn: integer;
    var score : real;
    score := 0.0;
    maxhitpoints := 60;
    hitpoints := maxhitpoints;
    turn := 1;
       
    var width, height : integer;
    

    width := 15;
    height := 15;    

    var max_monsters, monster_cnt : integer;
    max_monsters := 3;
    monster_cnt := 0;

    var monster_alive : array [max_monsters] of boolean;
    var monster_x, monster_y, monster_hitpoints : array [max_monsters] of integer;

    {* used when printing hud, to prevent map from jumping up and down*}
    {* Empty lines will be printed, if less messages were printed than msg_max *}
    var msg_cnt, msg_max : integer;
    msg_cnt := 0;
    msg_max := 5;


    var commands : array [10] of string;
    commands[0] := "1";
    commands[1] := "2";
    commands[2] := "3";
    commands[3] := "4";
    commands[4] := "5";
    commands[5] := "6";
    commands[6] := "7";
    commands[7] := "8";
    commands[8] := "9";
    commands[9] := "exit";

    {* Case insensitivity! *}
    var COMMAND_X_CHANGE, CoMmAnD_Y_cHaNge : array [9] of integer;

    command_x_change[0] := -1;
    command_y_change[0] := 1;

    command_x_change[1] := 0;
    command_y_change[1] := 1;
    
    command_x_change[2] := 1;
    command_y_change[2] := 1;
    
    command_x_change[3] := -1;
    command_y_change[3] := 0;
    
    command_x_change[4] := 0;
    command_y_change[4] := 0;

    command_x_change[5] := 1;
    command_y_change[5] := 0;
    
    command_x_change[6] := -1;
    command_y_change[6] := -1;
    
    command_x_change[7] := 0;
    command_y_change[7] := -1;
    
    command_x_change[8] := 1;
    command_y_change[8] := -1;

    var random_1, random_2, random_3 : integer;
    
    
    {* Utility functions *}
    
    {*
    RNDM.vim random number generator: http://fossies.org/linux/cream/Rndm.vim 
    It used to be a standard linear congruential generator, but the integer overflow (which is undefined behaviour)
    did not play nice with GCC optimizer 
    *}
    function random(max : integer) : integer;
    begin
        var random_4 : integer;
        random_4 := random_1 + random_2 + random_3;
        if random_2 < 50000000 then
            random_4 := random_4 + 1357;
        
        if  random_4 >= 100000000 then
            random_4 := random_4 - 100000000;

        if random_4 >= 100000000 then
            random_4 := random_4 - 100000000;
      
        random_1 := random_2;
        random_2 := random_3;
        random_3 := random_4;
        
        
        return random_3 % max; 
    end;
    
    procedure cls();
    begin
        writeln("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n");
    end;


    {* Converts x/y coordinate into a form that's usable with one dimensional array *}
    function get_coordinate(x : integer, y : integer, width : integer) : integer;
    begin
        return y*width + x;        
    end;

    function get_x_from_coordinate(coordinate : integer, width : integer) : integer;
    begin
        return coordinate % width;
    end;
    
    function get_y_from_coordinate(coordinate : integer, width : integer) : integer;
    begin
        return coordinate / width;
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
        writeln("Name: ", name, " Age: ", age, " HP: ", hitpoints, "/", maxhitpoints, " Score: ", score,  " Turn: ", turn); 
        while msg_cnt < msg_max do
        begin
            msg_cnt := msg_cnt + 1;
            writeln();
        end;

        msg_cnt := 0;
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

                procedure draw_cell();
                begin
                    {* Player has the highest priority *}
                    if (x = player_x) and (y = player_y) then
                    begin
                        line := line + "@";
                        return;
                    end;
                    {* Then monsters *}

                    var i : integer;
                    i := 0;
                    while i < max_monsters do
                    begin
                        if monster_alive[i] then
                        begin
                            if (monster_x[i] = x) and (monster_y[i] = y) then 
                            begin
                                line := line + "M";
                                return;
                            end;
                        end;
                        i := i + 1;
                    end;

                    {* Finally the walls *}               
                    if passability[get_coordinate(x, y, width)] then
                        line := line + " "
                    else
                        line := line + "#"; 
                end;

                draw_cell();
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

    function get_command_number(command : string) : integer;
    begin
        var i : integer;
        i := 0;
        while i < 9 do
        begin
            if commands[i] = command then
                return i;
            i := i + 1;
        end;        
        assert(false);
        return -1;  {* Control flow analysis isn't smart enough to realize that assert(false) terminates *}
    end;


    function read_command() : string;
    begin
        var command : string;

        while true do
        begin
            writeln("Input command (numpad 123456789 for direction, exit to quit):");
            read(command);
            if is_valid_command(command) then
                return command;
        end;
    end;

    procedure spawn_monster(passability : array [] of boolean, player_x : integer, player_y : integer);
    begin
        {* Always spawn monster on turn 2 *}
        
        var change, monster_spawn_prob : integer;
        monster_spawn_prob := 10 + turn / 3;
        change := random(100);

        if (turn <> 2) and (change >= monster_spawn_prob) then
            return;

        if monster_cnt >= max_monsters then
            return;

        function get_free_monster_slot() : integer;
        begin

            var i : integer;
            i := 0;
            while i < max_monsters do
            begin
                if not monster_alive[i] then
                    return i;                     
                i := i + 1;
            end;

            assert(false);
            return -1;
        end;

        var free_slot : integer;
        free_slot := get_free_monster_slot();
        monster_cnt := monster_cnt + 1;
        writeln("A monster appears...");
        msg_cnt := msg_cnt + 1;

        {* Strictly speaking not guaranteed to terminate *}
        var x, y : integer;
        while true do
        begin
            x := random(width);
            y := random(height);

            if passability[get_coordinate(x, y, width)] then
            begin
                if (x <> player_x) and (y <> player_y) then
                begin
                    var already_monster_present : boolean;
                    already_monster_present := false;
                    var i : integer;
                    i := 0;

                    while i < max_monsters do
                    begin
                        if monster_alive[i] and ((monster_x[i] = x) and (monster_y[i] = y)) then
                            already_monster_present := true;

                        i := i + 1;
                    end;

                    if not already_monster_present then
                    begin
                        monster_alive[free_slot] := true;
                        monster_hitpoints[free_slot] := random(20);
                        monster_x[free_slot] := x;
                        monster_y[free_slot] := y;
                        return;
                    end;
                end;
            end;
        end;
    end;


    procedure attack_monster(monster : integer);
    begin
        var damage : integer;
        {* 2d4 + 2 *}
        damage := 2*(random(4)+1) + 2;
        writeln("You attack the monster, dealing ", damage, " damage");
        msg_cnt := msg_cnt + 1;
        monster_hitpoints[monster] := monster_hitpoints[monster] - damage;
        if monster_hitpoints[monster] <= 0 then
        begin
            msg_cnt := msg_cnt + 1;
            writeln("The monster dies!");
            score := score + 4.0;
            monster_alive[monster] := false;
            monster_cnt := monster_cnt - 1;
        end;
    end;

    procedure update_player_position(passability : array [] of boolean, direction : integer, var player_x : integer, var player_y : integer);
    begin
        {* Don't move outside the map *}
        var new_x, new_y : integer;
        new_x := player_x + command_x_change[direction];
        new_y := player_y + command_y_change[direction];

        if (new_x < 0) or (new_x >= width) then
        return;

        if (new_y < 0) or (new_y >= height) then
            return;

        {* If monster is in the square, attack *}
        var i : integer;
        i := 0;

        while i < max_monsters do
        begin
            if monster_alive[i] then
            begin
                if (monster_x[i] = new_x) and (monster_y[i] = new_y) then
                begin
                    attack_monster(i);
                    return;
                end; 
            end;

            i := i + 1;
        end;

        if passability[get_coordinate(new_x, new_y, width)] then
        begin
            player_x := new_x;
            player_y := new_y;
        end;
    end;

    procedure move_monsters(passability : array [] of boolean, player_x : integer, player_y : integer, width : integer);
    begin
        {* breadth-first search. Somewhat complicated by the language constraints *}
        function find_path(start_x : integer, start_y : integer, width : integer) : integer;
        begin
            var parents, queue : array[width*height] of integer;
            {* Circular queue *}
            var i, queue_start, queue_end, queue_size : integer;
            i := 0;
            queue_size := 0;
            queue_start := 0;
            queue_end := 0;
            while i < parents.size do
            begin
                queue[i] := -1;
                parents[i] := -1;
                i := i + 1;
            end;

            procedure enqueue(i : integer);
            begin
                assert(queue_size < width*height);
                queue_size := queue_size + 1;
                queue[queue_end] := i;
                queue_end := (queue_end + 1) % (width * height);
            end;

            function dequeue() : integer;
            begin
                assert(queue_size > 0);
                queue_size := queue_size - 1;
                var value : integer;
                value := queue[queue_start];
                queue_start := (queue_start + 1) % (width * height);
                return value;
            end;

            var start_coordinate : integer;
            start_coordinate := get_coordinate(start_x, start_y, width);

            enqueue(start_coordinate);

            {* Actual BFS starts here *}
            while queue_size > 0 do
            begin
                var coord : integer;
                coord := dequeue();

                if coord = get_coordinate(player_x, player_y, width) then
                begin
                    while true do 
                    begin
                        var parent : integer;
                        parent := parents[coord];
                        if parent = start_coordinate then
                            return coord;

                        coord := parent;
                    end;
                end;

                var relx, rely : integer;
                relx := -1;
                rely := -1;

                {* Go through coordinate neighbours *}
                while rely <= 1 do
                begin
                    while relx <= 1 do
                    begin

                        procedure consider_coordinate();
                        begin
                            if (relx <> 0) or (rely <> 0) then
                            begin
                                var neighbour : integer; 
                                neighbour := coord;
                                var new_x, new_y : integer;
                                new_x := get_x_from_coordinate(neighbour, width) + relx;
                                new_y := get_y_from_coordinate(neighbour, width) + rely;

                                if (new_x < 0) or (new_x >= width) then
                                begin
                                    return;
                                end;
                                if (new_y < 0) or (new_y >= height) then
                                begin
                                    return;
                                end;

                                if not passability[get_coordinate(new_x, new_y, width)] then
                                begin
                                    return;
                                end;

                                neighbour := get_coordinate(new_x, new_y, width);
                            
                                if parents[neighbour] = -1 then
                                begin
                                    parents[neighbour] := coord;
                                    enqueue(neighbour);
                                end;
                            end;
                        end; {* End consider_coordinate *}
                        consider_coordinate();
                        relx := relx + 1;
                    end;

                    relx := -1;
                    rely := rely + 1;
                end;
            end;

            return -1;
        end; {* End find_path *}

        var i : integer;
        i := 0;
        while i < max_monsters do
        begin
            if monster_alive[i] then
            begin
                var next : integer;
                next := find_path(monster_x[i], monster_y[i], width);
                if next <> -1 then
                begin
                    if next = get_coordinate(player_x, player_y, width) then
                    begin
                        {* 1d8 *}
                        var damage : integer;
                        damage := random(8) + 1;
                        writeln("Monster hits you for ", damage, " damage");
                        hitpoints := hitpoints - damage;                        
                        msg_cnt := msg_cnt + 1;
                    end
                    else
                    begin
                        {* Check that monster is not blocking they way *}
                        var other_monster : integer;
                        other_monster := 0;
                        var blocking : boolean;
                        blocking := false;
                        while other_monster < max_monsters do
                        begin
                            if monster_alive[other_monster] and (other_monster <> i) then
                            begin
                                if get_coordinate(monster_x[other_monster], monster_y[other_monster], width) = next then
                                    blocking := true;
                            end;

                            other_monster := other_monster + 1;
                        end;    

                        if not blocking then
                        begin
                            monster_x[i] := get_x_from_coordinate(next, width);
                            monster_y[i] := get_y_from_coordinate(next, width);
                        end;
                    end;
                end;
            end;
            i := i + 1;
        end;
    end;

    procedure update_game_state(
        command : string, 
        var player_x : integer, 
        var player_y : integer, 
        passability : array [] of boolean, 
        width : integer,
        height : integer);
    begin


        if hitpoints < maxhitpoints then
        begin
            writeln("You heal a bit...");
            msg_cnt := msg_cnt + 1;
            hitpoints := hitpoints + 1;
        end;

        score := score - 0.1;
        assert(is_valid_command(command));
        var direction : integer;
        direction := get_command_number(command);

        update_player_position(passability, direction, player_x, player_y);
        spawn_monster(passability, player_x, player_y);
        move_monsters(passability, player_x, player_y, width);

        turn := turn + 1;
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
    {* Seed random number generator with age, as we do not have access to more sophisticated source of entropy *}
    random_1 := 32007779 + age;
    random_2 := 23717810  + age*2;
    random_3 := 52636370  + age*3;
    
    
    initialize_map(passability, width, height);
    passability[get_coordinate(player_x, player_y, width)] := true; {* player position *}

    cls();
    procedure main();
    begin
        var command : string;
        while true do
        begin

            print_hud();
            print_map(passability, player_x, player_y, width, height);

            command := read_command();

            if command = "exit" then
                return;

            cls();
            update_game_state(command, player_x, player_y, passability, width, height);
            if hitpoints <= 0 then
            begin
                writeln("YOU DIED!");
                return;
            end;
        end;
    end;  


    main();
end.