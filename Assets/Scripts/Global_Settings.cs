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
        public KeyCode fire1 = KeyCode.None;
        public KeyCode fire2 = KeyCode.None;
        public KeyCode fire3 = KeyCode.None;
        public KeyCode fire4 = KeyCode.None;
        public KeyCode shield = KeyCode.None;
        public KeyCode spawn = KeyCode.None;
    }

    public static Vector2 player_limits_x = new Vector2(-200f, 200f);
    public static Vector2 player_limits_y = new Vector2(-100f, 100f);
    public static Vector2 player_limits_death_y = new Vector2(-125f, 125f);

    public static float enemy_activation_distance = 140f;
    public static Vector2 enemy_limits_x = new Vector2(-220f, 220f);
    public static Vector2 enemy_limits_y = new Vector2(-140f, 140f);

    public static Vector2 bullet_limits_x = new Vector2(-220f, 220f);
    public static Vector2 bullet_limits_y = new Vector2(-140f, 140f);

    public static float Volume_SFX = 0.15f;
    public static float Volume_Music = 0.15f;

    public enum item_types { not_set, HP, Energy, Life, Money, EXP }

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
        Player_Controls[0].fire1 = KeyCode.LeftControl;
        Player_Controls[0].fire2 = KeyCode.LeftAlt;
        Player_Controls[0].fire3 = KeyCode.Space;
        Player_Controls[0].shield = KeyCode.LeftShift;
        Player_Controls[0].spawn = KeyCode.LeftControl;

        Player_Controls[1] = new Player_Controls_Info();
        Player_Controls[1].up = KeyCode.Keypad8;
        Player_Controls[1].down = KeyCode.Keypad2;
        Player_Controls[1].left = KeyCode.Keypad4;
        Player_Controls[1].right = KeyCode.Keypad6;
        Player_Controls[1].fire1 = KeyCode.X;
        Player_Controls[1].fire2 = KeyCode.Z;
        Player_Controls[1].fire3 = KeyCode.C;
        Player_Controls[1].shield = KeyCode.A;
        Player_Controls[1].spawn = KeyCode.A;
    }
}
