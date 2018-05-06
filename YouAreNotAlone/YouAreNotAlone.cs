using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Xml;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class YouAreNotAlone : Script
    {
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

        private List<EntitySet> replacedList;
        private List<EntitySet> carjackerList;
        private List<EntitySet> aggressiveList;
        private List<EntitySet> gangList;
        private List<EntitySet> massacreList;
        private List<EntitySet> racerList;
        private List<EntitySet> drivebyList;
        private List<EntitySet> terroristList;
        private static List<EntitySet> dispatchList;

        private float radius;
        private int eventTimeChecker;

        public enum CrimeType
        {
            None,
            AggressiveDriver,
            Carjacker,
            Driveby,
            GangTeam,
            Fire,
            Massacre,
            Racer,
            Terrorist
        }

        public enum EmergencyType
        {
            Army,
            Cop,
            Firefighter,
            Paramedic
        }

        static YouAreNotAlone()
        {
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
                new List<string> { "s_m_y_hwaycop_01", "s_f_y_sheriff_01", "s_m_y_sheriff_01", "s_f_y_ranger_01", "s_m_y_ranger_01" },
                new List<string> { "mp_g_m_pros_01" }
            };
            copCarNames = new List<List<string>>
            {
                new List<string> { "police", "police2", "police3", "policeb" },
                new List<string> { "sheriff", "sheriff2" },
                new List<string> { "police4" }
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
                "s_m_m_ciasec_01"
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
                "policet",
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
                "s_m_y_blackops_03",
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
                "firetruck"
            };
            dispatchList = new List<EntitySet>();

            DLCCheck();
            SetUp();
        }

        private static void DLCCheck()
        {
            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpapartment")))
            {
                racerCarNames.Add("verlierer2");
                drivebyCarNames.Add("baller3");
                armyHeliNames.Add("valkyrie2");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpassault")))
            {
                racerCarNames.Add("dominator3");
                racerCarNames.Add("ellie");
                racerCarNames.Add("entity2");
                racerCarNames.Add("flashgt");
                racerCarNames.Add("gb200");
                racerCarNames.Add("jester3");
                racerCarNames.Add("hotring");
                racerCarNames.Add("taipan");
                racerCarNames.Add("tezeract");
                racerCarNames.Add("tyrant");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpbiker")))
            {
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
                racerCarNames.Add("zentorno");
                drivebyCarNames.Add("huntley");
                drivebyCarNames.Add("thrust");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpchristmas2017")))
            {
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
                racerCarNames.Add("fmj");
                racerCarNames.Add("pfister811");
                racerCarNames.Add("prototipo");
                racerCarNames.Add("reaper");
                racerCarNames.Add("seven70");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpgunrunning")))
            {
                racerCarNames.Add("ardent");
                racerCarNames.Add("cheetah2");
                racerCarNames.Add("torero");
                racerCarNames.Add("vagner");
                racerCarNames.Add("xa21");
                terroristCarNames.Add("apc");
                swatCarNames.Add("nightshark");
                armyCarNames.Add("halftrack");
                armyCarNames.Add("insurgent3");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpheist")))
            {
                racerBikeNames.Add("lectro");
                drivebyCarNames.Add("enduro");
                fibModels.Add("s_m_m_fibsec_01");
                fibModels.Add("u_m_m_doa_01");
                armyCarNames.Add("barracks3");
                armyCarNames.Add("insurgent");
                armyCarNames.Add("insurgent2");
                armyHeliNames.Add("savage");
                armyHeliNames.Add("valkyrie");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpimportexport")))
            {
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
                racerCarNames.Add("banshee2");
                racerCarNames.Add("sultanrs");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mplowrider")))
            {
                drivebyCarNames.Add("buccaneer2");
                drivebyCarNames.Add("chino2");
                drivebyCarNames.Add("voodoo");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mplowrider2")))
            {
                drivebyCarNames.Add("faction3");
                drivebyCarNames.Add("sabregt2");
                drivebyCarNames.Add("virgo2");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mplts")))
            {
                racerBikeNames.Add("hakuchou");
                drivebyCarNames.Add("innovation");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpluxe")))
            {
                racerCarNames.Add("feltzer3");
                racerCarNames.Add("osiris");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpluxe2")))
            {
                racerCarNames.Add("t20");
                drivebyCarNames.Add("chino");
                drivebyCarNames.Add("vindicator");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpsmuggler")))
            {
                racerCarNames.Add("cyclone");
                racerCarNames.Add("rapidgt3");
                racerCarNames.Add("visione");
                armyHeliNames.Add("hunter");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpspecialraces")))
            {
                racerCarNames.Add("gp1");
                racerCarNames.Add("infernus2");
                racerCarNames.Add("turismo2");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "mpstunt")))
            {
                racerCarNames.Add("le7b");
                racerCarNames.Add("sheava");
                racerCarNames.Add("tyrus");
                drivebyCarNames.Add("bf400");
                drivebyCarNames.Add("gargoyle");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "spupgrade")))
            {
                drivebyCarNames.Add("stalion");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "pres")))
            {
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
                terroristCarNames.Add("apc2");
                copCarNames[0].Add("police6");
                copCarNames[0].Add("police8");
                swatCarNames.Add("nstockade");
                swatCarNames.Add("pstockade");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "wov")))
            {
                copModels[0].Add("s_m_y_bcop_01");
                copCarNames[1].Add("sheriff3");
                copHeliNames.Add("shemav");
                swatModels.Add("s_m_y_swat_02");
                swatModels.Add("s_m_y_swat_04");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "vwe")))
            {
                racerCarNames.Add("blista4");
                racerCarNames.Add("cheetah4");
                racerCarNames.Add("comet6");
                racerCarNames.Add("deluxo2");
                racerCarNames.Add("dominator4");
                racerCarNames.Add("elegy4");
                racerCarNames.Add("elegy5");
                racerCarNames.Add("elegy6");
                racerCarNames.Add("es550");
                racerCarNames.Add("euros");
                racerCarNames.Add("futo3");
                racerCarNames.Add("gauntlet3");
                racerCarNames.Add("gauntlet4");
                racerCarNames.Add("gauntlet5");
                racerCarNames.Add("hellhound");
                racerCarNames.Add("rapidgt4");
                racerCarNames.Add("requiem");
                racerCarNames.Add("tyrus2");
                racerCarNames.Add("vigero3");
                racerBikeNames.Add("kenshin");
                drivebyCarNames.Add("greenwood");
                copCarNames[1].Add("bcso1");
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
                copCarNames[1].Add("bcso4");
                copCarNames[1].Add("bcso5");
                copCarNames[1].Add("bcso6");
                copCarNames[0].Add("bpsp1");
                copCarNames[0].Add("bpsp2");
                copCarNames[0].Add("bulhway");
                copCarNames[0].Add("bulhway2");
                copCarNames[0].Add("bulpolice");
                copCarNames[0].Add("bulpolice2");
                copCarNames[1].Add("bulsheriff");
                copCarNames[0].Add("coqhway");
                copCarNames[0].Add("coqhway2");
                copCarNames[0].Add("coqpolice");
                copCarNames[1].Add("coqsheriff");
                copCarNames[0].Add("facthway");
                copCarNames[0].Add("infhway");
                copCarNames[0].Add("infpolice");
                copCarNames[0].Add("leesperanto");
                copCarNames[0].Add("pbp1");
                copCarNames[0].Add("pcpd1");
                copCarNames[0].Add("pcpd2");
                copCarNames[0].Add("pcpd3");
                copCarNames[0].Add("police9");
                copCarNames[0].Add("police11");
                copCarNames[0].Add("police12");
                copCarNames[0].Add("police13");
                copCarNames[0].Add("police15");
                copCarNames[0].Add("police16");
                copCarNames[0].Add("police17");
                copCarNames[0].Add("police24");
                copCarNames[0].Add("polizia1");
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
                copCarNames[0].Add("vighway");
                copCarNames[0].Add("vigpolice");
                copCarNames[1].Add("vigsheriff");
                fibCarNames.Add("fbi6");
                fibCarNames.Add("fbi7");
                fibCarNames.Add("fbi8");
                fibCarNames.Add("infpolice2");
                fibCarNames.Add("vaccapol");
                fibCarNames.Add("vcpd1");
                swatCarNames.Add("police14");
                swatCarNames.Add("policet2");
                swatCarNames.Add("policet3");
                emCarNames.Add("ambulance2");
                emCarNames.Add("ambulance3");
                emCarNames.Add("emssuv");
                emCarNames.Add("emsvan");
                fireCarNames.Add("emertruk");
                fireCarNames.Add("riot3");
            }

            if (Function.Call<bool>(Hash.IS_DLC_PRESENT, Function.Call<int>(Hash.GET_HASH_KEY, "dov")))
            {
                terroristCarNames.Add("dovnapc");
                copModels[2].Add("d_o_v_dick_01");
                copModels[0].Add("d_o_v_npatrol_01");
                copModels[0].Add("d_o_v_npatrol_02");
                copCarNames[2].Add("dovdtbuff");
                copCarNames[2].Add("dovdtfugi");
                copCarNames[2].Add("dovdtstan");
                copCarNames[0].Add("dovhpbuff2");
                copCarNames[0].Add("dovhpgran");
                copCarNames[0].Add("dovhpstan2");
                copCarNames[0].Add("dovngran");
                copCarNames[0].Add("dovpolesp");
                copCarNames[0].Add("dovpolfugi");
                copCarNames[0].Add("dovpolmerit");
                copCarNames[0].Add("dovpolstan");
                copCarNames[1].Add("dovshebuff");
                copCarNames[1].Add("dovsheesp");
                copCarNames[1].Add("dovsheranch");
                copCarNames[1].Add("dovshestan");
                copHeliNames.Add("dovpolmav");
                copHeliNames.Add("dovshemav");
                fibCarNames.Add("dovfibkur");
                fibCarNames.Add("dovfibranch");
                fibCarNames.Add("dovfibwash");
                swatCarNames.Add("dovnboxv");
                swatCarNames.Add("dovnrcv");
                swatCarNames.Add("dovnstock");
                swatCarNames.Add("dovnsurge");
                swatCarNames.Add("dovshetrans");
                emCarNames.Add("dovemambu");
                fireCarNames.Add("dovemfihvy");
            }
        }

        private static void SetUp()
        {
            XmlDocument doc = new XmlDocument();

            for (int time = 0; time < 500; time++)
            {
                doc.Load(@"scripts\\YouAreNotAlone.xml");

                if (doc != null) break;
            }

            if (doc == null) return;

            XmlElement element = doc.DocumentElement;

            foreach (XmlElement e in element.SelectNodes("//AddOn/spawn"))
            {
                string spawnName = e.GetAttribute("name");

                if (((Model)spawnName).IsValid) addOnCarNames.Add(spawnName);
            }

            foreach (XmlElement e in element.SelectNodes("//RaceCar/spawn"))
            {
                string spawnName = e.GetAttribute("name");

                if (((Model)spawnName).IsValid && ((Model)spawnName).IsCar) racerCarNames.Add(spawnName);
            }

            foreach (XmlElement e in element.SelectNodes("//RaceBike/spawn"))
            {
                string spawnName = e.GetAttribute("name");

                if (((Model)spawnName).IsValid && (((Model)spawnName).IsBike || ((Model)spawnName).IsQuadbike)) racerBikeNames.Add(spawnName);
            }

            foreach (XmlElement e in element.SelectNodes("//Driveby/spawn"))
            {
                string spawnName = e.GetAttribute("name");

                if (((Model)spawnName).IsValid) drivebyCarNames.Add(spawnName);
            }
        }

        public static void Dispatch(Entity target, CrimeType type)
        {
            Vector3 safePosition = Util.GetSafePositionNear(target);

            if (safePosition.Equals(Vector3.Zero)) return;

            switch (type)
            {
                case CrimeType.AggressiveDriver:
                case CrimeType.Racer:
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            int selectedType = Util.GetRandomInt(copCarNames.Count);
                            LSPD lspd = new LSPD(copCarNames[selectedType][Util.GetRandomInt(copCarNames[selectedType].Count)], target);

                            if (lspd.IsCreatedIn(safePosition, copModels[selectedType])) dispatchList.Add(lspd);
                            else lspd.Restore();
                        }

                        if (((Ped)target).IsSittingInVehicle() && ((Ped)target).CurrentVehicle.Model.IsCar)
                        {
                            LSPDHeli lspdheli = new LSPDHeli(copHeliNames[Util.GetRandomInt(copHeliNames.Count)], target);

                            if (lspdheli.IsCreatedIn(safePosition, copModels[Util.GetRandomInt(copModels.Count)])) dispatchList.Add(lspdheli);
                            else lspdheli.Restore();
                        }

                        break;
                    }

                case CrimeType.Carjacker:
                    {
                        int selectedType = Util.GetRandomInt(copCarNames.Count);
                        LSPD lspd = new LSPD(copCarNames[selectedType][Util.GetRandomInt(copCarNames[selectedType].Count)], target);

                        if (lspd.IsCreatedIn(safePosition, copModels[selectedType])) dispatchList.Add(lspd);
                        else lspd.Restore();

                        break;
                    }

                case CrimeType.Driveby:
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            int selectedType = Util.GetRandomInt(copCarNames.Count);
                            LSPD lspd = new LSPD(copCarNames[selectedType][Util.GetRandomInt(copCarNames[selectedType].Count)], target);

                            if (lspd.IsCreatedIn(safePosition, copModels[selectedType])) dispatchList.Add(lspd);
                            else lspd.Restore();
                        }

                        SWAT swat = new SWAT(swatCarNames[Util.GetRandomInt(swatCarNames.Count)], target);

                        if (swat.IsCreatedIn(safePosition, swatModels)) dispatchList.Add(swat);
                        else swat.Restore();

                        if (((Ped)target).IsSittingInVehicle() && ((Ped)target).CurrentVehicle.Model.IsCar)
                        {
                            SWATHeli swatheli = new SWATHeli(swatHeliNames[Util.GetRandomInt(swatHeliNames.Count)], target);

                            if (swatheli.IsCreatedIn(safePosition, swatModels)) dispatchList.Add(swatheli);
                            else swatheli.Restore();
                        }
                        else
                        {
                            LSPDHeli lspdheli = new LSPDHeli(copHeliNames[Util.GetRandomInt(copHeliNames.Count)], target);

                            if (lspdheli.IsCreatedIn(safePosition, copModels[Util.GetRandomInt(copModels.Count)])) dispatchList.Add(lspdheli);
                            else lspdheli.Restore();
                        }

                        break;
                    }

                case CrimeType.Fire:
                    {
                        if (!Util.AnyEmergencyIsNear(target.Position, EmergencyType.Firefighter))
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                Firefighter ff = new Firefighter(fireCarNames[Util.GetRandomInt(fireCarNames.Count)], target);

                                if (ff.IsCreatedIn(safePosition, fireModels)) dispatchList.Add(ff);
                                else ff.Restore();
                            }
                        }

                        if (!Util.AnyEmergencyIsNear(target.Position, EmergencyType.Paramedic))
                        {
                            Paramedic pm = new Paramedic(emCarNames[Util.GetRandomInt(emCarNames.Count)], target);

                            if (pm.IsCreatedIn(safePosition, emModels)) dispatchList.Add(pm);
                            else pm.Restore();
                        }

                        break;
                    }

                case CrimeType.GangTeam:
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            int selectedType = Util.GetRandomInt(copCarNames.Count);
                            LSPD lspd = new LSPD(copCarNames[selectedType][Util.GetRandomInt(copCarNames[selectedType].Count)], target);

                            if (lspd.IsCreatedIn(safePosition, copModels[selectedType])) dispatchList.Add(lspd);
                            else lspd.Restore();
                        }

                        LSPDHeli lspdheli = new LSPDHeli(copHeliNames[Util.GetRandomInt(copHeliNames.Count)], target);

                        if (lspdheli.IsCreatedIn(safePosition, copModels[Util.GetRandomInt(copModels.Count)])) dispatchList.Add(lspdheli);
                        else lspdheli.Restore();

                        break;
                    }

                case CrimeType.Massacre:
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            FIB fib = new FIB(fibCarNames[Util.GetRandomInt(fibCarNames.Count)], target);

                            if (fib.IsCreatedIn(safePosition, fibModels)) dispatchList.Add(fib);
                            else fib.Restore();

                            SWAT swat = new SWAT(swatCarNames[Util.GetRandomInt(swatCarNames.Count)], target);

                            if (swat.IsCreatedIn(safePosition, swatModels)) dispatchList.Add(swat);
                            else swat.Restore();
                        }

                        SWATHeli swatheli = new SWATHeli(swatHeliNames[Util.GetRandomInt(swatHeliNames.Count)], target);

                        if (swatheli.IsCreatedIn(safePosition, swatModels)) dispatchList.Add(swatheli);
                        else swatheli.Restore();

                        break;
                    }

                case CrimeType.Terrorist:
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            Army army = new Army(armyCarNames[Util.GetRandomInt(armyCarNames.Count)], target);

                            if (army.IsCreatedIn(safePosition, armyModels)) dispatchList.Add(army);
                            else army.Restore();
                        }

                        for (int i = 0; i < 2; i++)
                        {
                            ArmyHeli armyheli = new ArmyHeli(armyHeliNames[Util.GetRandomInt(armyHeliNames.Count)], target);

                            if (armyheli.IsCreatedIn(safePosition, armyModels)) dispatchList.Add(armyheli);
                            else armyheli.Restore();
                        }

                        break;
                    }
            }
        }

        public YouAreNotAlone()
        {
            replacedList = new List<EntitySet>();
            carjackerList = new List<EntitySet>();
            aggressiveList = new List<EntitySet>();
            gangList = new List<EntitySet>();
            massacreList = new List<EntitySet>();
            racerList = new List<EntitySet>();
            drivebyList = new List<EntitySet>();
            terroristList = new List<EntitySet>();

            radius = 100.0f;
            eventTimeChecker = 0;
            Tick += OnTick;
        }

        private void OnTick(Object sender, EventArgs e)
        {
            if (eventTimeChecker % 100 == 0)
            {
                CleanUp(replacedList);
                CleanUp(carjackerList);
                CleanUp(aggressiveList);
                CleanUp(gangList);
                CleanUp(massacreList);
                CleanUp(racerList);
                CleanUp(drivebyList);
                CleanUp(terroristList);
                CleanUp(dispatchList);
            }

            foreach (Nitroable en in aggressiveList) en.CheckNitroable();
            foreach (Nitroable en in racerList) en.CheckNitroable();

            if (eventTimeChecker == 800 || eventTimeChecker == 1600 || eventTimeChecker == 2400 || eventTimeChecker == 3600)
            {
                if (replacedList.Count < 7)
                {
                    ReplacedVehicle rv = new ReplacedVehicle(addOnCarNames[Util.GetRandomInt(addOnCarNames.Count)]);

                    if (rv.IsCreatedIn(radius))
                    {
                        replacedList.Add(rv);
                        Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                    }
                    else rv.Restore();
                }

                eventTimeChecker++;
            }
            else if (eventTimeChecker == 4000)
            {
                switch (Util.GetRandomInt(10))
                {
                    case 0:
                        {
                            Carjacker cj = new Carjacker();

                            if (cj.IsCreatedIn(radius))
                            {
                                carjackerList.Add(cj);
                                Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                            }
                            else cj.Restore();

                            break;
                        }

                    case 1:
                        {
                            AggressiveDriver ad = new AggressiveDriver(racerCarNames[Util.GetRandomInt(racerCarNames.Count)]);

                            if (ad.IsCreatedIn(radius))
                            {
                                aggressiveList.Add(ad);
                                Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                            }
                            else ad.Restore();

                            break;
                        }

                    case 2:
                        {
                            Vehicle[] nearbyVehicles = World.GetNearbyVehicles(Game.Player.Character.Position, radius);

                            if (nearbyVehicles.Length < 1)
                            {
                                nearbyVehicles = null;
                                break;
                            }

                            for (int trycount = 0; trycount < 5; trycount++)
                            {
                                Vehicle explosiveVehicle = nearbyVehicles[Util.GetRandomInt(nearbyVehicles.Length)];

                                if (Util.WeCanReplace(explosiveVehicle))
                                {
                                    if (Util.BlipIsOn(explosiveVehicle))
                                    {
                                        explosiveVehicle.CurrentBlip.Remove();
                                        Script.Wait(100);
                                    }

                                    Util.AddBlipOn(explosiveVehicle, 0.7f, BlipSprite.PersonalVehicleCar, BlipColor.Red, "Vehicle Explosion");
                                    Dispatch(explosiveVehicle, CrimeType.Fire);
                                    explosiveVehicle.Explode();
                                    Function.Call(Hash.FLASH_MINIMAP_DISPLAY);

                                    break;
                                }
                                else explosiveVehicle = null;
                            }

                            nearbyVehicles = null;
                            break;
                        }

                    case 3:
                        {
                            Vehicle[] nearbyVehicles = World.GetNearbyVehicles(Game.Player.Character.Position, radius);

                            if (nearbyVehicles.Length < 1)
                            {
                                nearbyVehicles = null;
                                break;
                            }

                            for (int trycount = 0; trycount < 5; trycount++)
                            {
                                Vehicle undriveableVehicle = nearbyVehicles[Util.GetRandomInt(nearbyVehicles.Length)];

                                if (Util.WeCanReplace(undriveableVehicle))
                                {
                                    if (Util.BlipIsOn(undriveableVehicle))
                                    {
                                        undriveableVehicle.CurrentBlip.Remove();
                                        Script.Wait(100);
                                    }

                                    Util.AddBlipOn(undriveableVehicle, 0.7f, BlipSprite.PersonalVehicleCar, BlipColor.Yellow, "Vehicle on Fire");
                                    Dispatch(undriveableVehicle, CrimeType.Fire);
                                    undriveableVehicle.EngineHealth = -900.0f;
                                    Function.Call(Hash.FLASH_MINIMAP_DISPLAY);

                                    break;
                                }
                                else undriveableVehicle = null;
                            }

                            nearbyVehicles = null;
                            break;
                        }

                    case 4:
                        {
                            Vector3 safePosition = Util.GetSafePositionIn(radius);

                            if (safePosition.Equals(Vector3.Zero)) break;

                            GangTeam teamA = new GangTeam();
                            GangTeam teamB = new GangTeam();

                            int teamANum = Util.GetRandomInt(gangModels.Count);
                            int teamBNum = -1;

                            while (teamBNum == -1 || teamANum == teamBNum) teamBNum = Util.GetRandomInt(gangModels.Count);

                            if (teamANum == -1 || teamBNum == -1) break;

                            int relationshipA = Util.NewRelationship(CrimeType.GangTeam);
                            int relationshipB = Util.NewRelationship(CrimeType.GangTeam);

                            if (relationshipA == 0 || relationshipB == 0) break;

                            World.SetRelationshipBetweenGroups(Relationship.Hate, relationshipA, relationshipB);
                            Vector3 position1 = World.GetNextPositionOnSidewalk(safePosition.Around(5.0f));
                            Vector3 position2 = World.GetNextPositionOnSidewalk(safePosition.Around(5.0f));

                            if (position1.Equals(Vector3.Zero) || position2.Equals(Vector3.Zero)) break;

                            if (teamA.IsCreatedIn(radius, position1, gangModels[teamANum], relationshipA, BlipColor.Green, "A Team")
                                && teamB.IsCreatedIn(radius, position2, gangModels[teamBNum], relationshipB, BlipColor.Red, "B Team"))
                            {
                                gangList.Add(teamA);
                                gangList.Add(teamB);

                                teamA.PerformTask();
                                teamB.PerformTask();

                                Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                            }
                            else
                            {
                                teamA.Restore();
                                teamB.Restore();
                            }

                            break;
                        }

                    case 5:
                        {
                            Vector3 safePosition = Util.GetSafePositionIn(radius);

                            if (safePosition.Equals(Vector3.Zero)) break;

                            Massacre ms = new Massacre();
                            int relationship = Util.NewRelationship(CrimeType.Massacre);

                            if (relationship == 0) break;
                            if (ms.IsCreatedIn(radius, safePosition, relationship))
                            {
                                massacreList.Add(ms);
                                Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                            }
                            else ms.Restore();

                            break;
                        }

                    case 6:
                        {
                            Vector3 goal = racingPosition[Util.GetRandomInt(racingPosition.Count)];
                            Vector3 safePosition = Util.GetSafePositionIn(radius);
                            int random = Util.GetRandomInt(4);

                            if (safePosition.Equals(Vector3.Zero)) break;

                            for (int i = 0; i < 4; i++)
                            {
                                Racer r = null;

                                if (random == 0) r = new Racer(racerBikeNames[Util.GetRandomInt(racerBikeNames.Count)], goal);
                                else r = new Racer(racerCarNames[Util.GetRandomInt(racerCarNames.Count)], goal);

                                Vector3 position = World.GetNextPositionOnStreet(safePosition, true);

                                if (!position.Equals(Vector3.Zero) && r.IsCreatedIn(radius, position))
                                {
                                    racerList.Add(r);
                                    Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                                }
                                else r.Restore();
                            }

                            break;
                        }

                    case 7:
                        {
                            Vehicle[] nearbyVehicles = World.GetNearbyVehicles(Game.Player.Character.Position, radius);

                            if (nearbyVehicles.Length < 1)
                            {
                                nearbyVehicles = null;
                                break;
                            }

                            for (int trycount = 0; trycount < 5; trycount++)
                            {
                                Vehicle tunedVehicle = nearbyVehicles[Util.GetRandomInt(nearbyVehicles.Length)];

                                if (Util.WeCanReplace(tunedVehicle) && Util.SomethingIsBetween(tunedVehicle) && !tunedVehicle.IsToggleModOn(VehicleToggleMod.Turbo))
                                {
                                    if (tunedVehicle.ClassType != VehicleClass.Sports && tunedVehicle.ClassType != VehicleClass.Super
                                        && tunedVehicle.ClassType != VehicleClass.SportsClassics && tunedVehicle.ClassType != VehicleClass.Coupes) continue;

                                    if (Util.BlipIsOn(tunedVehicle))
                                    {
                                        tunedVehicle.CurrentBlip.Remove();
                                        Script.Wait(100);
                                    }

                                    string blipName = "Tuned " + (tunedVehicle.FriendlyName == "NULL" ? tunedVehicle.DisplayName : tunedVehicle.FriendlyName);
                                    Util.AddBlipOn(tunedVehicle, 0.7f, BlipSprite.PersonalVehicleCar, (BlipColor)27, blipName);
                                    Util.Tune(tunedVehicle, true, true);
                                    Function.Call(Hash.FLASH_MINIMAP_DISPLAY);

                                    break;
                                }
                                else tunedVehicle = null;
                            }

                            nearbyVehicles = null;
                            break;
                        }

                    case 8:
                        {
                            Driveby db = new Driveby(drivebyCarNames[Util.GetRandomInt(drivebyCarNames.Count)]);

                            if (db.IsCreatedIn(radius, gangModels[Util.GetRandomInt(gangModels.Count)]))
                            {
                                drivebyList.Add(db);
                                Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                            }
                            else db.Restore();

                            break;
                        }

                    case 9:
                        {
                            Terrorist tr = new Terrorist(terroristCarNames[Util.GetRandomInt(terroristCarNames.Count)]);

                            if (tr.IsCreatedIn(radius))
                            {
                                terroristList.Add(tr);
                                Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                            }
                            else tr.Restore();

                            break;
                        }
                }

                eventTimeChecker = 0;
            }
            else eventTimeChecker++;
        }

        private void CleanUp(List<EntitySet> l)
        {
            for (int i = l.Count - 1; i >= 0; i--)
            {
                if (l[i].ShouldBeRemoved()) l.RemoveAt(i);
            }
        }
    }
}