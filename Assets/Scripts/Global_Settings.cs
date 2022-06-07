using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Global_Settings
{
    public static Player_Controls_Info[] Player_Controls = new Player_Controls_Info[]{ null, null, null, null };
    public class Player_Controls_Info {
        public KeyCode up = KeyCode.None;
        public KeyCode down = KeyCode.None;
        public KeyCode left = KeyCode.None;
        public KeyCode right = KeyCode.None;
        public KeyCode fire = KeyCode.None;
        public KeyCode shield = KeyCode.None;
        public KeyCode spawn = KeyCode.None;
    }

    public static Vector2 player_limits_x = new Vector2(-200f, 200f);
    public static Vector2 player_limits_y = new Vector2(-100f, 100f);
    public static Vector2 player_limits_death_y = new Vector2(-125f, 125f);

    public static float enemy_activation_distance = 130f;
    public static Vector2 enemy_limits_x = new Vector2(-250f, 250f);
    public static Vector2 enemy_limits_y = new Vector2(-170f, 170f);

    public static Vector2 bullet_limits_x = new Vector2(-300f, 300f);
    public static Vector2 bullet_limits_y = new Vector2(-200f, 200f);

    public static float Volume_SFX = 0.15f;
    public static float Volume_Music = 0.15f;

    static bool is_initialized = false;

    // Start is called before the first frame update
    public static void Init()
    {
        if (is_initialized) return;
        is_initialized = true;

        Player_Controls[0] = new Player_Controls_Info();
        Player_Controls[0].up = KeyCode.UpArrow;
        Player_Controls[0].down = KeyCode.DownArrow;
        Player_Controls[0].left = KeyCode.LeftArrow;
        Player_Controls[0].right = KeyCode.RightArrow;
        Player_Controls[0].fire = KeyCode.LeftControl;
        Player_Controls[0].shield = KeyCode.LeftShift;
        Player_Controls[0].spawn = KeyCode.LeftControl;

        Player_Controls[1] = new Player_Controls_Info();
        Player_Controls[1].up = KeyCode.Keypad8;
        Player_Controls[1].down = KeyCode.Keypad2;
        Player_Controls[1].left = KeyCode.Keypad4;
        Player_Controls[1].right = KeyCode.Keypad6;
        Player_Controls[1].fire = KeyCode.X;
        Player_Controls[1].shield = KeyCode.Z;
        Player_Controls[1].spawn = KeyCode.X;
    }
}
