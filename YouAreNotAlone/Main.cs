using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Xml;

namespace YouAreNotAlone
{
    public class Main : Script
    {
        public static bool NoDispatch;
        public static bool CriminalsCanFightWithPlayer;
        public static bool DispatchesCanFightWithPlayer;
        public static bool NoMinimapFlash;
        public static bool NoBlipOnCriminal;
        public static bool NoBlipOnDispatch;
        public static bool NoLog;

        private static List<string> addOnCarNames;
        private static List<string> racerCarNames;
        private static List<string> racerBikeNames;
        private static List<string> drivebyCarNames;
        private static List<string> terroristCarNames;
        private static List<Vector3> racingPosition;
        private static List<List<string>> gangModels;

        private static List<List<string>> copModels;
        private static List<List<string>> copCarNames;
        private static List<string> copHeliNames;
        private static List<string> fibModels;
        private static List<string> fibCarNames;
        private static List<string> swatModels;
        private static List<string> swatCarNames;
        private static List<string> swatHeliNames;
        private static List<string> armyModels;
        private static List<string> armyCarNames;
        private static List<string> armyHeliNames;
        private static List<string> emModels;
        private static List<string> emCarNames;
        private static List<string> fireModels;
        private static List<string> fireCarNames;

        private float radius;
        private int eventTimeChecker;

        static Main()
        {
            Logger.Init();
            Util.Init();
            VehicleInfo.Init();

            NoDispatch = false;
            CriminalsCanFightWithPlayer = false;
            DispatchesCanFightWithPlayer = false;
            NoMinimapFlash = false;
            NoBlipOnCriminal = false;
            NoBlipOnDispatch = false;
            NoLog = true;

            addOnCarNames = new List<string>();
            racerCarNames = new List<string>
            {
                "banshee",
                "cheetah",
                "elegy2",
                "entityxf",
                "infernus",
                "jester",
                "sentinel2",
                "turismor",
                "vacca"
            };
            racerBikeNames = new List<string>
            {
                "akuma",
                "bati",
                "bati2",
                "carbonrs",
                "double",
                "nemesis",
                "pcj",
                "ruffian",
                "vader"
            };
            drivebyCarNames = new List<string>
            {
                "baller",
                "bison",
                "bodhi2",
                "buccaneer",
                "cavalcade2",
                "daemon",
                "dubsta",
                "emperor",
                "glendale",
                "granger",
                "minivan",
                "patriot",
                "rancherxl",
                "regina",
                "sadler",
                "sanchez",
                "superd",
                "tailgater",
                "warrener"
            };
            terroristCarNames = new List<string>
            {
                "rhino"
            };
            racingPosition = new List<Vector3>
            {
                new Vector3(811.49f, 1275.29f, 360.51f),
                new Vector3(-410.44f, 1174.3f, 325.64f),
                new Vector3(-2318.34f, 283.7f, 169.47f),
                new Vector3(-3019.15f, 89.12f, 11.61f),
                new Vector3(1673.18f, -66.4f, 173.68f),
                new Vector3(2509.46f, -285.34f, 92.99f),
                new Vector3(2774.5f, -710.7f, 6.18f),
                new Vector3(2718.0f, 1352.04f, 24.52f),
                new Vector3(2310.91f, 2571.45f, 46.67f),
                new Vector3(2413.66f, 3100.91f, 48.15f),
                new Vector3(2478.14f, 3823.33f, 40.52f),
                new Vector3(3326.59f, 5152.29f, 18.29f),
                new Vector3(-1121.59f, 4925.97f, 218.6f),
                new Vector3(2203.89f, 5574.07f, 53.72f),
                new Vector3(1598.11f, 6551.38f, 14.0f),
                new Vector3(39.24f, 6287.88f, 31.26f),
                new Vector3(-1579.7f, 5167.46f, 19.55f),
                new Vector3(-2349.96f, 3421.71f, 29.06f),
                new Vector3(-1577.33f, 2102.62f, 67.95f),
                new Vector3(-1610.24f, 178.93f, 59.67f),
                new Vector3(-1734.18f, -1108.4f, 13.07f),
                new Vector3(1193.08f, -2908.43f, 5.47f),
                new Vector3(1753.45f, -1537.28f, 112.14f),
                new Vector3(-3152.32f, 1093.24f, 20.71f),
                new Vector3(-9.46f, 3042.79f, 40.72f),
                new Vector3(-546.07f, -2812.95f, 5.57f),
                new Vector3(-962.02f, -3006.96f, 13.95f),
                new Vector3(1411.63f, 3012.39f, 40.53f),
                new Vector3(338.6f, 3564.52f, 33.5f),
                new Vector3(597.45f, 613.62f, 128.91f),
                new Vector3(1378.63f, -2071.99f, 52.0f)
            };
            gangModels = new List<List<string>>
            {
                new List<string> { "a_m_m_og_boss_01", "mp_m_famdd_01", "g_f_y_families_01", "g_m_y_famca_01", "g_m_y_famdnf_01", "g_m_y_famfor_01" },
                new List<string> { "g_f_y_ballas_01", "g_m_y_ballaeast_01", "g_m_y_ballaorig_01", "g_m_y_ballasout_01" },
                new List<string> { "a_m_y_mexthug_01", "g_m_y_mexgoon_01", "g_m_y_mexgoon_02", "g_m_y_mexgoon_03", "g_f_y_vagos_01", "g_m_m_mexboss_01", "g_m_m_mexboss_02", "g_m_y_mexgang_01" },
                new List<string> { "g_m_y_lost_01", "g_m_y_lost_02", "g_m_y_lost_03", "g_f_y_lost_01", "g_m_m_armboss_01", "g_m_m_armgoon_01", "g_m_m_armlieut_01", "g_m_y_armgoon_02" },
                new List<string> { "g_m_y_azteca_01", "g_m_y_salvaboss_01", "g_m_y_salvagoon_01", "g_m_y_salvagoon_02", "g_m_y_salvagoon_03" },
                new List<string> { "g_m_y_korean_01", "g_m_y_korean_02", "g_m_y_korlieut_01", "g_m_m_korboss_01", "a_m_y_ktown_01", "a_m_y_ktown_02" },
                new List<string> { "a_m_m_eastsa_01", "a_m_m_eastsa_02", "a_m_m_malibu_01", "a_m_m_mexcntry_01", "a_m_m_mexlabor_01", "a_m_m_polynesian_01", "a_m_m_soucent_01", "a_m_m_soucent_03" },
                new List<string> { "a_m_m_stlat_02", "s_m_m_bouncer_01", "u_m_m_aldinapoli", "u_m_m_bikehire_01", "u_m_m_rivalpap", "u_m_m_willyfist", "u_m_y_baygor", "u_m_y_chip" },
                new List<string> { "s_m_y_dealer_01", "u_m_y_tattoo_01", "u_m_y_sbike", "u_m_y_party_01", "u_m_y_hippie_01", "u_m_y_gunvend_01", "u_m_y_fibmugger_01", "u_m_y_guido_01" },
                new List<string> { "s_f_y_hooker_01", "s_f_y_hooker_02", "s_f_y_hooker_03", "s_f_y_stripper_01", "s_f_y_stripper_02" }
            };

            copModels = new List<List<string>>
            {
                new List<string> { "s_f_y_cop_01", "s_m_y_cop_01" },
                new List<string> { "s_f_y_sheriff_01", "s_m_y_sheriff_01", "s_f_y_ranger_01", "s_m_y_ranger_01" },
                new List<string> { "s_m_m_highsec_01", "s_m_m_highsec_02" },
                new List<string> { "s_m_y_hwaycop_01" }
            };
            copCarNames = new List<List<string>>
            {
                new List<string> { "police", "police2", "police3", "policet" },
                new List<string> { "sheriff", "sheriff2" },
                new List<string> { "police4" },
                new List<string> { "policeb" }
            };
            copHeliNames = new List<string>
            {
                "buzzard2",
                "polmav"
            };
            fibModels = new List<string>
            {
                "s_m_m_fiboffice_01",
                "s_m_m_fiboffice_02",
                "s_m_m_ciasec_01",
                "mp_m_fibsec_01"
            };
            fibCarNames = new List<string>
            {
                "fbi",
                "fbi2"
            };
            swatModels = new List<string>
            {
                "s_m_y_swat_01"
            };
            swatCarNames = new List<string>
            {
                "riot"
            };
            swatHeliNames = new List<string>
            {
                "buzzard",
                "frogger"
            };
            armyModels = new List<string>
            {
                "s_m_y_blackops_01",
                "s_m_y_blackops_02",
                "s_m_y_marine_01",
                "s_m_y_marine_03"
            };
            armyCarNames = new List<string>
            {
                "barracks",
                "crusader"
            };
            armyHeliNames = new List<string>
            {
                "annihilator"
            };
            emModels = new List<string>
            {
                "s_m_m_paramedic_01"
            };
            emCarNames = new List<string>
            {
                "ambulance"
            };
            fireModels = new List<string>
            {
                "s_m_y_fireman_01"
            };
            fireCarNames = new List<string>
            {
                "firetruk"
            };

            Logger.Write(false, "Main: Added default models.", "");
            CheckDLCs();
            SetUp();
        }

        private static void CheckDLCs()
        {
            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpapartment")))
            {
                Logger.Write(false, "Main: Found MPApartment.", "");
                racerCarNames.Add("verlierer2");
                drivebyCarNames.Add("baller3");
                armyHeliNames.Add("valkyrie2");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpassault")))
            {
                Logger.Write(false, "Main: Found MPAssault.", "");
                racerCarNames.Add("dominator3");
                racerCarNames.Add("ellie");
                racerCarNames.Add("entity2");
                racerCarNames.Add("flashgt");
                racerCarNames.Add("gb200");
                racerCarNames.Add("hotring");
                racerCarNames.Add("jester3");
                racerCarNames.Add("taipan");
                racerCarNames.Add("tezeract");
                racerCarNames.Add("tyrant");
                drivebyCarNames.Add("caracara");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpbattle")))
            {
                Logger.Write(false, "Main: Found MPBattle.", "");
                racerCarNames.Add("swinger");
                armyCarNames.Add("menacer");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpbiker")))
            {
                Logger.Write(false, "Main: Found MPBiker.", "");
                racerBikeNames.Add("defiler");
                racerBikeNames.Add("hakuchou2");
                racerBikeNames.Add("shotaro");
                racerBikeNames.Add("vortex");
                drivebyCarNames.Add("manchez");
                drivebyCarNames.Add("nightblade");
                drivebyCarNames.Add("wolfsbane");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpbusiness2")))
            {
                Logger.Write(false, "Main: Found MPBusiness2.", "");
                racerCarNames.Add("zentorno");
                drivebyCarNames.Add("huntley");
                drivebyCarNames.Add("thrust");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpchristmas2017")))
            {
                Logger.Write(false, "Main: Found MPChristmans2017.", "");
                racerCarNames.Add("autarch");
                racerCarNames.Add("comet5");
                racerCarNames.Add("deluxo");
                racerCarNames.Add("gt500");
                racerCarNames.Add("neon");
                racerCarNames.Add("pariah");
                racerCarNames.Add("savestra");
                racerCarNames.Add("sc1");
                racerCarNames.Add("sentinel3");
                racerCarNames.Add("stromberg");
                racerCarNames.Add("viseris");
                racerCarNames.Add("z190");
                drivebyCarNames.Add("hermes");
                terroristCarNames.Add("khanjali");
                swatCarNames.Add("riot2");
                armyCarNames.Add("barrage");
                armyHeliNames.Add("akula");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpexecutive")))
            {
                Logger.Write(false, "Main: Found MPExecutive.", "");
                racerCarNames.Add("fmj");
                racerCarNames.Add("pfister811");
                racerCarNames.Add("prototipo");
                racerCarNames.Add("reaper");
                racerCarNames.Add("seven70");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpgunrunning")))
            {
                Logger.Write(false, "Main: Found MPGunrunning.", "");
                racerCarNames.Add("ardent");
                racerCarNames.Add("cheetah2");
                racerCarNames.Add("torero");
                racerCarNames.Add("vagner");
                racerCarNames.Add("xa21");
                terroristCarNames.Add("apc");
                armyCarNames.Add("halftrack");
                armyCarNames.Add("insurgent3");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpheist")))
            {
                Logger.Write(false, "Main: Found MPHeist.", "");
                racerBikeNames.Add("lectro");
                drivebyCarNames.Add("enduro");
                fibModels.Add("s_m_m_fibsec_01");
                fibModels.Add("u_m_m_doa_01");
                armyModels.Add("s_m_y_blackops_03");
                armyCarNames.Add("barracks3");
                armyCarNames.Add("insurgent");
                armyCarNames.Add("insurgent2");
                armyHeliNames.Add("savage");
                armyHeliNames.Add("valkyrie");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpimportexport")))
            {
                Logger.Write(false, "Main: Found MPImportExport.", "");
                racerCarNames.Add("comet3");
                racerCarNames.Add("elegy");
                racerCarNames.Add("italigtb");
                racerCarNames.Add("italigtb2");
                racerCarNames.Add("nero");
                racerCarNames.Add("nero2");
                racerCarNames.Add("penetrator");
                racerCarNames.Add("tempesta");
                racerBikeNames.Add("diablous");
                racerBikeNames.Add("diablous2");
                racerBikeNames.Add("fcr");
                racerBikeNames.Add("fcr2");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpjanuary2016")))
            {
                Logger.Write(false, "Main: Found MPJanuary2016.", "");
                racerCarNames.Add("banshee2");
                racerCarNames.Add("sultanrs");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mplowrider")))
            {
                Logger.Write(false, "Main: Found MPLowrider.", "");
                drivebyCarNames.Add("buccaneer2");
                drivebyCarNames.Add("chino2");
                drivebyCarNames.Add("voodoo");
                gangModels[2].Add("csb_vagspeak");
                gangModels[2].Add("ig_vagspeak");
                gangModels[2].Add("mp_m_g_vagfun_01");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mplowrider2")))
            {
                Logger.Write(false, "Main: Found MPLowrider2.", "");
                drivebyCarNames.Add("faction3");
                drivebyCarNames.Add("sabregt2");
                drivebyCarNames.Add("virgo2");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mplts")))
            {
                Logger.Write(false, "Main: Found MPLTS.", "");
                racerBikeNames.Add("hakuchou");
                drivebyCarNames.Add("innovation");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpluxe")))
            {
                Logger.Write(false, "Main: Found MPLuxe.", "");
                racerCarNames.Add("feltzer3");
                racerCarNames.Add("osiris");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpluxe2")))
            {
                Logger.Write(false, "Main: Found MPLuxe2.", "");
                racerCarNames.Add("t20");
                drivebyCarNames.Add("chino");
                drivebyCarNames.Add("vindicator");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpsmuggler")))
            {
                Logger.Write(false, "Main: Found MPSmuggler.", "");
                racerCarNames.Add("cyclone");
                racerCarNames.Add("rapidgt3");
                racerCarNames.Add("visione");
                armyHeliNames.Add("hunter");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpspecialraces")))
            {
                Logger.Write(false, "Main: Found MPSpecialRaces.", "");
                racerCarNames.Add("gp1");
                racerCarNames.Add("infernus2");
                racerCarNames.Add("turismo2");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpstunt")))
            {
                Logger.Write(false, "Main: Found MPStunt.", "");
                racerCarNames.Add("le7b");
                racerCarNames.Add("sheava");
                racerCarNames.Add("tyrus");
                drivebyCarNames.Add("bf400");
                drivebyCarNames.Add("gargoyle");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "spupgrade")))
            {
                Logger.Write(false, "Main: Found SPUpgrade.", "");
                drivebyCarNames.Add("stalion");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "pres")))
            {
                Logger.Write(false, "Main: Found IVPack.", "");
                racerCarNames.Add("cheetah3");
                racerCarNames.Add("sentinel4");
                racerCarNames.Add("supergt");
                racerCarNames.Add("turismo");
                racerCarNames.Add("typhoon");
                racerCarNames.Add("uranus");
                racerBikeNames.Add("double2");
                racerBikeNames.Add("hakuchou3");
                racerBikeNames.Add("nrg900");
                drivebyCarNames.Add("huntley2");
                drivebyCarNames.Add("marbelle");
                armyCarNames.Add("apc2");
                copCarNames[0].Add("police6");
                copCarNames[0].Add("police8");
                swatCarNames.Add("nstockade");
                swatCarNames.Add("pstockade");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "wov")))
            {
                Logger.Write(false, "Main: Found World of Variety.", "");
                copModels[3].Add("s_m_y_bcop_01");
                copCarNames[1].Add("sheriff3");
                copHeliNames.Add("shemav");
                swatHeliNames.Add("fibmav");
                swatModels.Add("s_m_y_swat_02");
                swatModels.Add("s_m_y_swat_04");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "vwe")))
            {
                Logger.Write(false, "Main: Found Vanillaworks Extended.", "");
                racerCarNames.Add("blista4");
                racerCarNames.Add("cheetah4");
                racerCarNames.Add("comet6");
                racerCarNames.Add("deluxo2");
                racerCarNames.Add("dominator4");
                racerCarNames.Add("elegy4");
                racerCarNames.Add("elegy5");
                racerCarNames.Add("elegy6");
                racerCarNames.Add("es550");
                racerCarNames.Add("futo3");
                racerCarNames.Add("gauntlet3");
                racerCarNames.Add("gauntlet5");
                racerCarNames.Add("hellhound");
                racerCarNames.Add("rapidgt4");
                racerCarNames.Add("tyrus2");
                racerCarNames.Add("vigero3");
                racerBikeNames.Add("kenshin");
                drivebyCarNames.Add("greenwood");
                copCarNames[1].Add("bcso2");
                copCarNames[1].Add("bcso3");
                copCarNames[0].Add("hwaycar5");
                copCarNames[0].Add("hwaycar6");
                copCarNames[0].Add("police18");
                copCarNames[0].Add("police19");
                copCarNames[1].Add("sheriff10");
                copCarNames[1].Add("sheriff11");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "dw")))
            {
                Logger.Write(false, "Main: Found Dispatchworks.", "");
                copCarNames[1].Add("bcso4");
                copCarNames[1].Add("bcso5");
                copCarNames[1].Add("bcso6");
                copCarNames[0].Add("bulhway");
                copCarNames[0].Add("bulhway2");
                copCarNames[0].Add("bulpolice");
                copCarNames[0].Add("bulpolice2");
                copCarNames[2].Add("bulpolice3");
                copCarNames[1].Add("bulsheriff");
                copCarNames[0].Add("coqhway");
                copCarNames[0].Add("coqhway2");
                copCarNames[0].Add("coqpolice");
                copCarNames[2].Add("coqpolice2");
                copCarNames[1].Add("coqsheriff");
                copCarNames[0].Add("facthway");
                copCarNames[0].Add("infhway");
                copCarNames[0].Add("infpolice");
                copCarNames[2].Add("infpolice2");
                copCarNames[0].Add("leesperanto");
                copCarNames[0].Add("pbp1");
                copCarNames[0].Add("pcpd1");
                copCarNames[0].Add("pcpd2");
                copCarNames[0].Add("pcpd3");
                copCarNames[0].Add("police9");
                copCarNames[0].Add("police11");
                copCarNames[0].Add("police12");
                copCarNames[0].Add("police13");
                copCarNames[2].Add("police14");
                copCarNames[0].Add("police15");
                copCarNames[0].Add("police16");
                copCarNames[2].Add("police20");
                copCarNames[2].Add("police21");
                copCarNames[2].Add("police22");
                copCarNames[0].Add("police24");
                copCarNames[2].Add("police25");
                copCarNames[0].Add("policet2");
                copCarNames[0].Add("policet3");
                copCarNames[1].Add("pranger3");
                copCarNames[0].Add("rpdcar1");
                copCarNames[2].Add("rpdcar2");
                copCarNames[1].Add("rpdcar3");
                copCarNames[0].Add("rpdsuv");
                copCarNames[0].Add("rpdsuv2");
                copCarNames[1].Add("sheriff7");
                copCarNames[1].Add("sheriff9");
                copCarNames[1].Add("uranushway");
                copCarNames[1].Add("uranushway2");
                copCarNames[2].Add("vaccapol");
                copCarNames[2].Add("vcpd1");
                copCarNames[0].Add("vighway");
                copCarNames[0].Add("vigpolice");
                copCarNames[2].Add("vigpolice2");
                copCarNames[1].Add("vigsheriff");
                fibCarNames.Add("fbi6");
                fibCarNames.Add("fbi7");
                fibCarNames.Add("fbi8");
                emCarNames.Add("ambulance2");
                emCarNames.Add("ambulance3");
                fireCarNames.Add("riot3");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "dov")))
            {
                Logger.Write(false, "Main: Found Dispatch of Variety.", "");
                armyCarNames.Add("dovnapc");
                copModels[2].Add("d_o_v_dick_01");
                copModels[3].Add("d_o_v_npatrol_01");
                copModels[0].Add("d_o_v_npatrol_02");
                copCarNames[2].Add("dovdtbuff");
                copCarNames[2].Add("dovdtstan");
                copCarNames[0].Add("dovhpbuff2");
                copCarNames[0].Add("dovhpgran");
                copCarNames[0].Add("dovhpstan");
                copCarNames[0].Add("dovhpstan2");
                copCarNames[0].Add("dovngran");
                copCarNames[0].Add("dovpolfugi");
                copCarNames[0].Add("dovpolmerit");
                copCarNames[0].Add("dovpolstan");
                copCarNames[1].Add("dovshebuff");
                copCarNames[1].Add("dovsheesp");
                copCarNames[1].Add("dovsheranch");
                copCarNames[1].Add("dovshestan");
                copCarNames[1].Add("dovshetrans");
                copHeliNames.Add("dovpolmav");
                copHeliNames.Add("dovshemav");
                fibCarNames.Add("dovfibkur");
                fibCarNames.Add("dovfibranch");
                fibCarNames.Add("dovfibwash");
                swatCarNames.Add("dovnboxv");
                swatCarNames.Add("dovnrcv");
                swatCarNames.Add("dovnstock");
                swatCarNames.Add("dovnsurge");
                emCarNames.Add("dovemambu");
            }
        }

        private static void SetUp()
        {
            XmlDocument doc = new XmlDocument();

            for (int time = 0; time < 500; time++)
            {
                doc.Load(@"scripts\\YouAreNotAlone.xml");

                if (doc != null)
                {
                    Logger.Write(false, "Main: Found XML file.", "");

                    break;
                }
            }

            if (doc == null)
            {
                Logger.Write(false, "Main: Couldn't find XML file.", "");

                return;
            }

            XmlElement element = doc.DocumentElement;

            NoDispatch = ((XmlElement)element.SelectSingleNode("//Settings/NoDispatch")).GetAttribute("value") == "True";
            CriminalsCanFightWithPlayer = ((XmlElement)element.SelectSingleNode("//Settings/CriminalsCanFightWithPlayer")).GetAttribute("value") == "True";
            DispatchesCanFightWithPlayer = ((XmlElement)element.SelectSingleNode("//Settings/DispatchesCanFightWithPlayer")).GetAttribute("value") == "True";
            NoMinimapFlash = ((XmlElement)element.SelectSingleNode("//Settings/NoMinimapFlash")).GetAttribute("value") == "True";
            NoBlipOnCriminal = ((XmlElement)element.SelectSingleNode("//Settings/NoBlipOnCriminal")).GetAttribute("value") == "True";
            NoBlipOnDispatch = ((XmlElement)element.SelectSingleNode("//Settings/NoBlipOnDispatch")).GetAttribute("value") == "True";
            NoLog = ((XmlElement)element.SelectSingleNode("//Settings/NoLog")).GetAttribute("value") == "True";

            foreach (XmlElement e in element.SelectNodes("//AddOn/spawn"))
            {
                string spawnName = e.GetAttribute("name");

                if (((Model)spawnName).IsInCdImage && (((Model)spawnName).IsCar || ((Model)spawnName).IsBicycle || ((Model)spawnName).IsBike || ((Model)spawnName).IsQuadbike)) addOnCarNames.Add(spawnName);
            }

            foreach (XmlElement e in element.SelectNodes("//RaceCar/spawn"))
            {
                string spawnName = e.GetAttribute("name");

                if (((Model)spawnName).IsInCdImage && ((Model)spawnName).IsCar) racerCarNames.Add(spawnName);
            }

            foreach (XmlElement e in element.SelectNodes("//RaceBike/spawn"))
            {
                string spawnName = e.GetAttribute("name");

                if (((Model)spawnName).IsInCdImage && (((Model)spawnName).IsBike || ((Model)spawnName).IsQuadbike)) racerBikeNames.Add(spawnName);
            }

            foreach (XmlElement e in element.SelectNodes("//Driveby/spawn"))
            {
                string spawnName = e.GetAttribute("name");

                if (((Model)spawnName).IsInCdImage && (((Model)spawnName).IsCar || ((Model)spawnName).IsBike || ((Model)spawnName).IsQuadbike)) drivebyCarNames.Add(spawnName);
            }

            Logger.Write(false, "Main: Completed setting.", "");
        }

        public static bool DispatchAgainst(Entity target, EventManager.EventType type)
        {
            if (NoDispatch) return false;

            Vector3 safePosition = Util.GetSafePositionNear(target.Position + target.ForwardVector * 70.0f);

            if (safePosition.Equals(Vector3.Zero)) return false;

            int minOfSuccess = 0, success = 0;

            switch (type)
            {
                case EventManager.EventType.AggressiveDriver:
                case EventManager.EventType.Racer:
                    {
                        minOfSuccess = 1;

                        for (int i = 0; i < 2; i++)
                        {
                            int selectedType = Util.GetRandomIntBelow(copCarNames.Count);
                            EmergencyGround lspd = new EmergencyGround(copCarNames[selectedType][Util.GetRandomIntBelow(copCarNames[selectedType].Count)], target, "LSPD");

                            if (lspd.IsCreatedIn(safePosition, copModels[selectedType]) && DispatchManager.Add(lspd, DispatchManager.DispatchType.CopGround)) success++;
                            else lspd.Restore(true);
                        }

                        if (target.Model.IsPed && ((Ped)target).IsSittingInVehicle() && ((Ped)target).CurrentVehicle.Model.IsCar)
                        {
                            minOfSuccess = 2;
                            EmergencyHeli lspdheli = new EmergencyHeli(copHeliNames[Util.GetRandomIntBelow(copHeliNames.Count)], target, "LSPD");

                            if (lspdheli.IsCreatedIn(safePosition, copModels[Util.GetRandomIntBelow(copModels.Count)]) && DispatchManager.Add(lspdheli, DispatchManager.DispatchType.CopHeli)) success++;
                            else lspdheli.Restore(true);
                        }

                        break;
                    }

                case EventManager.EventType.Carjacker:
                    {
                        int selectedType = Util.GetRandomIntBelow(copCarNames.Count);
                        EmergencyGround lspd = new EmergencyGround(copCarNames[selectedType][Util.GetRandomIntBelow(copCarNames[selectedType].Count)], target, "LSPD");

                        if (lspd.IsCreatedIn(safePosition, copModels[selectedType]) && DispatchManager.Add(lspd, DispatchManager.DispatchType.CopGround)) success++;
                        else lspd.Restore(true);

                        break;
                    }

                case EventManager.EventType.Driveby:
                    {
                        minOfSuccess = 1;

                        for (int i = 0; i < 2; i++)
                        {
                            int selectedType = Util.GetRandomIntBelow(copCarNames.Count);
                            EmergencyGround lspd = new EmergencyGround(copCarNames[selectedType][Util.GetRandomIntBelow(copCarNames[selectedType].Count)], target, "LSPD");

                            if (lspd.IsCreatedIn(safePosition, copModels[selectedType]) && DispatchManager.Add(lspd, DispatchManager.DispatchType.CopGround)) success++;
                            else lspd.Restore(true);
                        }

                        if (target.Model.IsPed && ((Ped)target).IsSittingInVehicle() && ((Ped)target).CurrentVehicle.Model.IsCar)
                        {
                            minOfSuccess = 3;
                            EmergencyGround swat = new EmergencyGround(swatCarNames[Util.GetRandomIntBelow(swatCarNames.Count)], target, "SWAT");

                            if (swat.IsCreatedIn(safePosition, swatModels) && DispatchManager.Add(swat, DispatchManager.DispatchType.CopGround)) success++;
                            else swat.Restore(true);

                            EmergencyHeli swatheli = new EmergencyHeli(swatHeliNames[Util.GetRandomIntBelow(swatHeliNames.Count)], target, "SWAT");

                            if (swatheli.IsCreatedIn(safePosition, swatModels) && DispatchManager.Add(swatheli, DispatchManager.DispatchType.CopHeli)) success++;
                            else swatheli.Restore(true);
                        }

                        break;
                    }

                case EventManager.EventType.Fire:
                    {
                        Firefighter ff = new Firefighter(fireCarNames[Util.GetRandomIntBelow(fireCarNames.Count)], target);

                        if (ff.IsCreatedIn(safePosition, fireModels) && DispatchManager.Add(ff, DispatchManager.DispatchType.Emergency)) success++;
                        else ff.Restore(true);

                        Paramedic pm = new Paramedic(emCarNames[Util.GetRandomIntBelow(emCarNames.Count)], target);

                        if (pm.IsCreatedIn(safePosition, emModels) && DispatchManager.Add(pm, DispatchManager.DispatchType.Emergency)) success++;
                        else pm.Restore(true);

                        break;
                    }

                case EventManager.EventType.GangTeam:
                    {
                        minOfSuccess = 2;

                        for (int i = 0; i < 3; i++)
                        {
                            int selectedType = Util.GetRandomIntBelow(copCarNames.Count);
                            EmergencyGround lspd = new EmergencyGround(copCarNames[selectedType][Util.GetRandomIntBelow(copCarNames[selectedType].Count)], target, "LSPD");

                            if (lspd.IsCreatedIn(safePosition, copModels[selectedType]) && DispatchManager.Add(lspd, DispatchManager.DispatchType.CopGround)) success++;
                            else lspd.Restore(true);
                        }

                        break;
                    }

                case EventManager.EventType.Massacre:
                    {
                        minOfSuccess = 4;

                        for (int i = 0; i < 2; i++)
                        {
                            EmergencyGround fib = new EmergencyGround(fibCarNames[Util.GetRandomIntBelow(fibCarNames.Count)], target, "FIB");

                            if (fib.IsCreatedIn(safePosition, fibModels) && DispatchManager.Add(fib, DispatchManager.DispatchType.CopGround)) success++;
                            else fib.Restore(true);

                            EmergencyGround swat = new EmergencyGround(swatCarNames[Util.GetRandomIntBelow(swatCarNames.Count)], target, "SWAT");

                            if (swat.IsCreatedIn(safePosition, swatModels) && DispatchManager.Add(swat, DispatchManager.DispatchType.CopGround)) success++;
                            else swat.Restore(true);

                            EmergencyHeli swatheli = new EmergencyHeli(swatHeliNames[Util.GetRandomIntBelow(swatHeliNames.Count)], target, "SWAT");

                            if (swatheli.IsCreatedIn(safePosition, swatModels) && DispatchManager.Add(swatheli, DispatchManager.DispatchType.CopHeli)) success++;
                            else swatheli.Restore(true);
                        }

                        break;
                    }

                case EventManager.EventType.Terrorist:
                    {
                        minOfSuccess = 4;

                        for (int i = 0; i < 4; i++)
                        {
                            EmergencyGround army = new EmergencyGround(armyCarNames[Util.GetRandomIntBelow(armyCarNames.Count)], target, "ARMY");

                            if (army.IsCreatedIn(safePosition, armyModels) && DispatchManager.Add(army, DispatchManager.DispatchType.ArmyGround)) success++;
                            else army.Restore(true);
                        }

                        for (int i = 0; i < 2; i++)
                        {
                            EmergencyHeli armyheli = new EmergencyHeli(armyHeliNames[Util.GetRandomIntBelow(armyHeliNames.Count)], target, "ARMY");

                            if (armyheli.IsCreatedIn(safePosition, armyModels) && DispatchManager.Add(armyheli, DispatchManager.DispatchType.ArmyHeli)) success++;
                            else armyheli.Restore(true);
                        }

                        break;
                    }
            }

            return success >= minOfSuccess;
        }

        public static bool BlockRoadAgainst(Entity target, EventManager.EventType type)
        {
            if (NoDispatch) return true;

            Vector3 safePosition = Util.GetSafePositionNear(target.Position + target.ForwardVector * 50.0f);

            if (safePosition.Equals(Vector3.Zero)) return false;

            switch (type)
            {
                case EventManager.EventType.AggressiveDriver:
                case EventManager.EventType.Racer:
                    {
                        int selectedType = Util.GetRandomIntBelow(copCarNames.Count - 1);
                        EmergencyBlock lspdblock = new EmergencyBlock(copCarNames[selectedType][Util.GetRandomIntBelow(copCarNames[selectedType].Count)], target, "LSPD");

                        if (lspdblock.IsCreatedIn(safePosition, copModels[selectedType])) return DispatchManager.Add(lspdblock, DispatchManager.DispatchType.CopRoadBlock);
                        else lspdblock.Restore(true);

                        break;
                    }

                case EventManager.EventType.Driveby:
                    {
                        EmergencyBlock swatblock = new EmergencyBlock(swatCarNames[Util.GetRandomIntBelow(swatCarNames.Count)], target, "SWAT");

                        if (swatblock.IsCreatedIn(safePosition, swatModels)) return DispatchManager.Add(swatblock, DispatchManager.DispatchType.CopRoadBlock);
                        else swatblock.Restore(true);

                        break;
                    }

                case EventManager.EventType.Terrorist:
                    {
                        EmergencyBlock armyblock = new EmergencyBlock(armyCarNames[Util.GetRandomIntBelow(armyCarNames.Count)], target, "ARMY");

                        if (armyblock.IsCreatedIn(safePosition, armyModels)) return DispatchManager.Add(armyblock, DispatchManager.DispatchType.ArmyRoadBlock);
                        else armyblock.Restore(true);

                        break;
                    }
            }

            return false;
        }

        public Main()
        {
            radius = 100.0f;
            eventTimeChecker = 0;
            Interval = 15000;
            Tick += OnTick;
            Logger.Write(false, "YouAreNotAlone started.", "");
        }

        private void OnTick(Object sender, EventArgs e)
        {
            if (eventTimeChecker == 1 || eventTimeChecker == 2 || eventTimeChecker == 3 || eventTimeChecker == 4)
            {
                if (EventManager.ReplaceSlotIsAvailable() && addOnCarNames.Count > 0)
                {
                    ReplacedVehicle rv = new ReplacedVehicle(addOnCarNames[Util.GetRandomIntBelow(addOnCarNames.Count)]);

                    if (rv.IsCreatedIn(radius) && EventManager.Add(rv, EventManager.EventType.ReplacedVehicle))
                    {
                        if (!NoMinimapFlash) Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                    }
                    else rv.Restore(true);
                }

                eventTimeChecker++;
            }
            else if (eventTimeChecker == 5)
            {
                switch (Util.GetRandomIntBelow(9))
                {
                    case 0:
                        {
                            Carjacker cj = new Carjacker();

                            if (cj.IsCreatedIn(radius) && EventManager.Add(cj, EventManager.EventType.Carjacker))
                            {
                                if (!NoMinimapFlash) Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                            }
                            else cj.Restore(true);

                            break;
                        }

                    case 1:
                        {
                            AggressiveDriver ad = new AggressiveDriver(racerCarNames[Util.GetRandomIntBelow(racerCarNames.Count)]);

                            if (ad.IsCreatedIn(radius) && EventManager.Add(ad, EventManager.EventType.AggressiveDriver))
                            {
                                if (!NoMinimapFlash) Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                            }
                            else ad.Restore(true);

                            break;
                        }

                    case 2:
                        {
                            OnFire of = new OnFire();

                            if (of.IsCreatedIn(radius, true) && EventManager.Add(of, EventManager.EventType.Fire))
                            {
                                if (!NoMinimapFlash) Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                            }
                            else of.Restore(true);

                            break;
                        }

                    case 3:
                        {
                            OnFire of = new OnFire();

                            if (of.IsCreatedIn(radius, false) && EventManager.Add(of, EventManager.EventType.Fire))
                            {
                                if (!NoMinimapFlash) Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                            }
                            else of.Restore(true);

                            break;
                        }

                    case 4:
                        {
                            Vector3 safePosition = Util.GetSafePositionIn(radius);

                            if (safePosition.Equals(Vector3.Zero)) break;

                            Road road = new Road(Vector3.Zero, 0.0f);

                            for (int cnt = 0; cnt < 5; cnt++)
                            {
                                road = Util.GetNextPositionOnStreetWithHeading(safePosition.Around(50.0f));

                                if (!road.Position.Equals(Vector3.Zero)) break;
                            }

                            if (road.Position.Equals(Vector3.Zero)) break;

                            GangTeam teamA = new GangTeam();
                            GangTeam teamB = new GangTeam();

                            int teamANum = Util.GetRandomIntBelow(gangModels.Count);
                            int teamBNum = -1;

                            while (teamBNum == -1 || teamANum == teamBNum) teamBNum = Util.GetRandomIntBelow(gangModels.Count);

                            if (teamANum == -1 || teamBNum == -1) break;

                            int relationshipA = Util.NewRelationshipOf(EventManager.EventType.GangTeam);
                            int relationshipB = Util.NewRelationshipOf(EventManager.EventType.GangTeam);

                            if (relationshipA == 0 || relationshipB == 0) break;

                            World.SetRelationshipBetweenGroups(Relationship.Hate, relationshipA, relationshipB);

                            if (teamA.IsCreatedIn(radius, road.Position.Around(5.0f), gangModels[teamANum], relationshipA, BlipColor.Green, "A Team")
                                && teamB.IsCreatedIn(radius, road.Position.Around(5.0f), gangModels[teamBNum], relationshipB, BlipColor.Red, "B Team")
                                && EventManager.Add(teamA, EventManager.EventType.GangTeam) && EventManager.Add(teamB, EventManager.EventType.GangTeam))
                            {
                                teamA.PerformTask();
                                teamB.PerformTask();

                                if (!NoMinimapFlash) Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                            }
                            else
                            {
                                teamA.Restore(true);
                                teamB.Restore(true);
                            }

                            break;
                        }

                    case 5:
                        {
                            Vector3 safePosition = Util.GetSafePositionIn(radius);

                            if (safePosition.Equals(Vector3.Zero)) break;

                            Road road = new Road(Vector3.Zero, 0.0f);

                            for (int cnt = 0; cnt < 5; cnt++)
                            {
                                road = Util.GetNextPositionOnStreetWithHeading(safePosition.Around(50.0f));

                                if (!road.Position.Equals(Vector3.Zero)) break;
                            }

                            if (road.Position.Equals(Vector3.Zero)) break;

                            Massacre ms = new Massacre();

                            if (ms.IsCreatedIn(radius, road.Position) && EventManager.Add(ms, EventManager.EventType.Massacre))
                            {
                                if (!NoMinimapFlash) Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                            }
                            else ms.Restore(true);

                            break;
                        }

                    case 6:
                        {
                            if (racingPosition.Count < 1) break;

                            Vector3 goal = racingPosition[Util.GetRandomIntBelow(racingPosition.Count)];
                            Vector3 safePosition = Util.GetSafePositionIn(radius);

                            if (safePosition.Equals(Vector3.Zero)) break;

                            Racers r = null;

                            if (Util.GetRandomIntBelow(4) == 1) r = new Racers(racerBikeNames, safePosition, goal);
                            else r = new Racers(racerCarNames, safePosition, goal);

                            if (r.IsCreatedIn(radius) && EventManager.Add(r, EventManager.EventType.Racer))
                            {
                                if (!NoMinimapFlash) Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                            }
                            else r.Restore(true);

                            break;
                        }

                    case 7:
                        {
                            Driveby db = new Driveby(drivebyCarNames[Util.GetRandomIntBelow(drivebyCarNames.Count)]);

                            if (db.IsCreatedIn(radius, gangModels[Util.GetRandomIntBelow(gangModels.Count)]) && EventManager.Add(db, EventManager.EventType.Driveby))
                            {
                                if (!NoMinimapFlash) Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                            }
                            else db.Restore(true);

                            break;
                        }

                    case 8:
                        {
                            Terrorist tr = new Terrorist(terroristCarNames[Util.GetRandomIntBelow(terroristCarNames.Count)]);

                            if (tr.IsCreatedIn(radius) && EventManager.Add(tr, EventManager.EventType.Terrorist))
                            {
                                if (!NoMinimapFlash) Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                            }
                            else tr.Restore(true);

                            break;
                        }
                }

                eventTimeChecker = 0;
            }
            else eventTimeChecker++;
        }
    }
}