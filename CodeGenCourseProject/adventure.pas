program adventure_game;
begin
    {* Global game state *}
    var name : string;
    var age, random_num : integer;
    var hitpoints, maxhitpoints, turn: integer;
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
    var monster_x, monster_y : array [max_monsters] of integer;



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

            writeln("Invalid command '", command, "'"); 

        end;
    end;

    procedure spawn_monster(passability : array [] of boolean, player_x : integer, player_y : integer);
    begin
        {* Always spawn monster on turn 2 *}
        
        var change, monster_spawn_prob : integer;
        monster_spawn_prob := 10;
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
                        monster_x[free_slot] := x;
                        monster_y[free_slot] := y;
                        return;
                    end;
                end;
            end;
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
        assert(is_valid_command(command));
        var direction : integer;
        direction := get_command_number(command);

        if (player_x + command_x_change[direction] < 0) or (player_x + command_x_change[direction] >= width) then
            return;

        if (player_y + command_y_change[direction] < 0) or (player_y + command_y_change[direction] >= height) then
            return;

        if passability[get_coordinate(player_x + command_x_change[direction], player_y + command_y_change[direction], width)] then
        begin
            player_x := player_x + command_x_change[direction];
            player_y := player_y + command_y_change[direction];
        end;

        spawn_monster(passability, player_x, player_y);

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
    {* Seed random number generator with age, as we do not have access to more sophisticated source of enthropy *}
    random_num := age;
    initialize_map(passability, width, height);
    passability[get_coordinate(player_x, player_y, width)] := true; {* player position *}

    procedure main();
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

            update_game_state(command, player_x, player_y, passability, width, height);

        end;
    end;  


    main();
end.