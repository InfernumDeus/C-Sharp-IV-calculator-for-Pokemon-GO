/* Pokemon GO IV and level calculator v1.0
created: 11.05.2017
author: InfernumDeus
e-mail: sataha19@mail.ru
github: https://github.com/InfernumDeus

Feel free to use it in your projects and modify in any way.*/

using System;
using System.Collections.Generic;
using System.Linq;

using CPmult;

namespace IVcalc
{
    class Program
    {    
        //stardust, min lvl id, max lvl id
        public static List<Tuple<int, int, int>> stardustChart = new List<Tuple<int, int, int>> {
            Tuple.Create(200,   0,  3),
            Tuple.Create(400,   4,  7),
            Tuple.Create(600,   8,  11),
            Tuple.Create(800,   12, 15),
            Tuple.Create(1000,  16, 19),
            Tuple.Create(1300,  20, 23),
            Tuple.Create(1600,  24, 27),
            Tuple.Create(1900,  28, 31),
            Tuple.Create(2200,  32, 35),
            Tuple.Create(2500,  36, 39),
            Tuple.Create(3000,  40, 43),
            Tuple.Create(3500,  44, 47),
            Tuple.Create(4000,  48, 51),
            Tuple.Create(4500,  52, 55),
            Tuple.Create(5000,  56, 59),
            Tuple.Create(6000,  60, 63),
            Tuple.Create(7000,  64, 67),
            Tuple.Create(8000,  68, 71),
            Tuple.Create(9000,  72, 75),
            Tuple.Create(10000, 76, 77),
            Tuple.Create(0,     78, 78)
        };

        static void Main(string[] args) {
            List<int[]> possible_sets = new List<int[]>();
            possible_sets = calculate_IV(2606, 233, 4500, 
                                         190, 190, 320, 
                                         true, false, 4, 4, 
                                         false, true, false);

            if (possible_sets.Count == 0) Console.WriteLine("Couldn't find valid level + IV set");
            else
            Console.WriteLine("Possible sets (level, attack, defence, stamina):");
            foreach (int[] set in possible_sets)
            {
                Console.Write(cp_mult.ID_to_level(set[0]) + " ");
                Console.Write(set[1] + " ");
                Console.Write(set[2] + " ");
                Console.WriteLine(set[3]);
            }
            Console.ReadKey();
        }

        //no answers from messages by default
        //result array = {level, attackIV, defenceIV, staminaIV}
        static List<int[]> calculate_IV(int cp, int hp, int stardust, 
                                        int baseAtk, int baseDef, int baseSta,
                                        bool poweredUp = true, bool hatched = false,
                                        int message_overall = -1, 
                                        int message_best_stat = -1,
                                        bool atk_is_max = false, 
                                        bool def_is_max = false, 
                                        bool sta_is_max = false)
        {
            int minAtk = 0; int maxAtk = 45;
            int minDef = 0; int maxDef = 45;
            int minSta = 0; int maxSta = 45;
            
            if (hatched)
            {
                if (minAtk < 10) minAtk = 10;
                if (minDef < 10) minDef = 10;
                if (minSta < 10) minSta = 10;
            }

            int minLvlID = 0; int maxLvlID = 78;
            if (stardust != -1)
                for (int i = 0; i <= 20; i++)
                    if (stardustChart[i].Item1 == stardust)
                    {
                        minLvlID = stardustChart[i].Item2;
                        maxLvlID = stardustChart[i].Item3;
                        break;
                    }

            int minSumIV = 0; int maxSumIV = 45;
            switch (message_overall)
            {
                case 1: minSumIV = 0; maxSumIV = 22; break;
                case 2: minSumIV = 23; maxSumIV = 29; break;
                case 3: minSumIV = 30; maxSumIV = 36; break;
                case 4: minSumIV = 37; maxSumIV = 45; break;
                default: minSumIV = 0; maxSumIV = 45; break;
            }

            //calculate possible level and stamina by hp
            List<Tuple<int, int>> lvlID_and_IVsta_List = new List<Tuple<int, int>>();
            for (int sta = minSta; sta <= maxSta; sta++)
            {
                //// test Digglet data
                /*hp = 10;
                cp = 10; 

                baseAtk = 109;
                baseDef = 88;
                baseSta = 20;

                minLvlID = 0;
                maxLvlID = 3;*/
                ////
                
                float possible_cp_mult = (float)hp / (float)(baseSta + sta);
                float closest = cp_mult.m.Aggregate((x, y) => Math.Abs(x - possible_cp_mult) < Math.Abs(y - possible_cp_mult) ? x : y);

                int minLvlID_for_this_IV = cp_mult.m.IndexOf(closest);

                int calculated_hp = (int)Math.Floor((float)(baseSta + sta) * cp_mult.m[minLvlID_for_this_IV]);
                if (calculated_hp < 10) calculated_hp = 10;

                //if calculated value invalid
                //check if +- 0.5 level might be valid
                if (hp != calculated_hp)
                { 
                    if (minLvlID_for_this_IV != 0)
                    { //check lower level
                        calculated_hp = (int)Math.Floor((float)(baseSta + sta) * cp_mult.m[minLvlID_for_this_IV - 1]);
                        if (calculated_hp < 10) calculated_hp = 10;
                        if (hp == calculated_hp)
                        {
                            minLvlID_for_this_IV--;
                            goto valueFound;
                        }
                    }

                    if (minLvlID_for_this_IV != 78)
                    { //check higher level
                        calculated_hp = (int)Math.Floor((float)(baseSta + sta) * cp_mult.m[minLvlID_for_this_IV + 1]);
                        if (calculated_hp < 10) calculated_hp = 10;
                        if (hp == calculated_hp)
                        {
                            minLvlID_for_this_IV++;
                            goto valueFound;
                        }
                    }
                    continue; 
                }
                valueFound:

                int maxLvlID_for_this_IV = minLvlID_for_this_IV;

                //find min lvl ID
                if (minLvlID_for_this_IV != 0)
                {
                    int next_check_lvl = minLvlID_for_this_IV - 1;
                    calculated_hp = (int)Math.Floor((float)(baseSta + sta) * cp_mult.m[next_check_lvl]);
                    if (calculated_hp < 10) calculated_hp = 10;
                    while (hp == calculated_hp)
                    {
                        if (hp == 10) //every level below is gonna have 10 hp
                        { 
                            minLvlID_for_this_IV = 0;
                            break;
                        }
                        minLvlID_for_this_IV = next_check_lvl;
                        next_check_lvl--;
                        if (next_check_lvl < 0) break;
                        calculated_hp = (int)Math.Floor((float)(baseSta + sta) * cp_mult.m[next_check_lvl]);
                        if (calculated_hp < 10) calculated_hp = 10;
                    }
                }

                //find max lvl ID
                if (maxLvlID_for_this_IV != 78)
                {
                    int next_check_lvl = minLvlID_for_this_IV + 1;
                    calculated_hp = (int)Math.Floor((float)(baseSta + sta) * cp_mult.m[next_check_lvl]);
                    if (calculated_hp < 10) calculated_hp = 10;
                    while (hp == calculated_hp)
                    {
                        maxLvlID_for_this_IV = next_check_lvl;
                        next_check_lvl++;
                        if (next_check_lvl > 78) break;
                        calculated_hp = (int)Math.Floor((float)(baseSta + sta) * cp_mult.m[next_check_lvl]);
                        if (calculated_hp < 10) calculated_hp = 10;
                    }
                }
                
                //create list of valid "level + staminaIV" sets
                for (int lvlID = minLvlID_for_this_IV; lvlID <= maxLvlID_for_this_IV; lvlID++)
                    if (poweredUp || lvlID % 2 == 0)
                        if (lvlID >= minLvlID && lvlID <= maxLvlID)
                            lvlID_and_IVsta_List.Add(new Tuple<int, int>(lvlID, sta));
            }
            
            List<int[]> LADS = new List<int[]>();
            for (int atk = minAtk;
                     atk <= maxAtk;
                     atk++)
            {
                for (int def = minDef;
                         def <= maxDef;
                         def++)
                {
                    foreach (Tuple<int, int> t in lvlID_and_IVsta_List)
                    {
                        if (atk + def + t.Item2 < minSumIV &&
                            atk + def + t.Item2 > maxSumIV) continue;

                        //avoid IV sets with invalid highest IV
                        int maxValue = new[] { atk, def, t.Item2 }.Max();
                        switch (message_best_stat)
                        {
                            case 1: if (maxValue > 7) continue; break; //0-7
                            case 2: if (maxValue < 8 || maxValue > 12) continue; break; //8-12
                            case 3: if (maxValue < 13 || maxValue > 14) continue; break; //13-14
                            case 4: if (maxValue != 15) continue; break; //15
                            default: continue;
                        }

                        if ((atk == maxValue ^ atk_is_max) ||
                            (def == maxValue ^ def_is_max) ||
                            (t.Item2 == maxValue ^ sta_is_max))
                            continue;
                        ///////

                        float cpm = cp_mult.m[t.Item1];
                        int calculated_cp = (int)Math.Floor(Math.Sqrt((float)(baseSta + t.Item2) * cpm)
                                                          * Math.Sqrt((float)(baseDef + def) * cpm)
                                                          * (float)(baseAtk + atk) * cpm
                                                          / 10);
                        if (calculated_cp < 10) calculated_cp = 10;

                        if (calculated_cp == cp)
                        {
                            LADS.Add(new[] { t.Item1, atk, def, t.Item2 });
                        }
                    }
                }
            }
            return LADS;
        }
    }
}
