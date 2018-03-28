using System;
using System.Collections.Generic;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;
using MySql.Data.MySqlClient;
using System.Data;
using System.Timers;
using System.IO;
/*
 * Проверить U autobuy
 * Проверить продажу машины
 * Сделать меню лидера для принятие и увольнение фермеров.
 * В меню лидера добавить возможность изменения цены за продукт.
 * Добавить анимацию бросания в машину.
 * Сделать менюшки.
 * Добавить проверку на то, что нельзя пригласить фермером самого себя
 */
namespace _4_lifeRP
{
    class JobFarm : Script
    {
        public static bool playerEntered = false;
        public class farmInfo // Класс для представления информации о ферме в виртуальном мире
        {
            public string owner;
            public int farmID;
            public int farmPrice;
            public int warehouse;
            public string productType;
            public int productPrice;
            public List<string> farmers = new List<string>();

            public farmInfo(string itsOwner, int itsFarmID, int itsFarmPrice, int itsWareHouse, string itsProductType, int itsProductPrice, List<string> itsFarmers)
            {
                owner = itsOwner;
                farmID = itsFarmID;
                farmPrice = itsFarmPrice;
                warehouse = itsWareHouse;
                productType = itsProductType;
                productPrice = itsProductPrice;
                farmers = itsFarmers;
            }
            public void updateFarm()
            {
                string allfarmers = ""; // фермеры через запятую
                foreach (string f in farmers)
                {
                    allfarmers = allfarmers + f + ",";
                }
                string resultFarmers = allfarmers;
                if (allfarmers.Length > 0)
                {
                    resultFarmers = allfarmers.Remove((allfarmers.Length - 1));
                }
                string queryString = string.Format(@"UPDATE `farm` SET `owner`='{0}', `farmID`='{1}', `farmPrice`='{2}', `warehouse`='{3}', `productType`='{4}',
                                                `productPrice`= '{5}', `farmers`='{6}' WHERE `farmID` = '{7}'",
                                      owner, farmID, farmPrice, warehouse, productType, productPrice, resultFarmers, farmID);
                lifeRP_GM.mainClass.sqlCon.retSQLData(queryString);
            }
        }
        
        public class farmCarInfo // Класс для представления информации о конкретной машине
        {
            public VehInfo car;
            public int farmID; // какой ферме принадлежит
            public int greenhouseID; // 0 ни одной теплице не принадлежит
            public int productCount;
            public bool isWorking; // if true => стоит и ждет рабочих if false => не работает, загрузка невозможна.

            public farmCarInfo(VehInfo itsCar, int itsFarmID, int itsGreenhouseID, int itsProductCount, bool itsIsWorking)
            {
                car = itsCar;
                farmID = itsFarmID;
                greenhouseID = itsGreenhouseID;
                productCount = itsProductCount;
                isWorking = itsIsWorking;
            }
        }
        public const int maxWarehouse = 100000; // максимальное кол-во продуктов на складе фермы
        public static List<farmInfo> allFarms = new List<farmInfo>();
        public static List<VehInfo> farmCars = new List<VehInfo>();
        public static List<farmCarInfo> farmCarInfos = new List<farmCarInfo>();
        public static List<MarkerInfo> farmMarkers = new List<MarkerInfo>();
        public static List<MarkerInfo> farmGreenhouses = new List<MarkerInfo>();
        public static List<MarkerInfo> farmPickProducts = new List<MarkerInfo>(); // маркера выхода
        public static List<MarkerInfo> farmStorehouses = new List<MarkerInfo>();
        public static List<TLabelInfo> farmGreenhouseLabels = new List<TLabelInfo>();

        private System.Timers.Timer timerFirstAnim;
        private System.Timers.Timer timerSecondAnim;
		
		//TODO: Сделать загрузку координат каждой теплицы с xml
        #region farmOne 
        private static List<Vector3> farmOneGreenhouse_ONE_PickMarkers = new List<Vector3>() // теплица 1
        {
            new Vector3(390.10, 6645.42, 27.66),
            new Vector3(392.28, 6636.42, 27.69),
            new Vector3(390.10, 6625.03, 27.55),
            new Vector3(392.52, 6609.78, 27.50),
            new Vector3(390.35, 6607.55, 27.48),
            new Vector3(392.60, 6602.71, 27.41),
            new Vector3(390.10, 6599.40, 27.40)
        };
        private static List<Vector3> farmOneGreenhouse_TWO_PickMarkers = new List<Vector3>() // теплица 2
        {
            new Vector3(381.45, 6645.28, 27.81),
            new Vector3(383.99, 6642.61, 27.73),
            new Vector3(386.17, 6645.42, 27.69),
            new Vector3(386.01, 6639.59, 27.75),
            new Vector3(381.63, 6638.13, 27.82),
            new Vector3(383.88, 6635.50, 27.78),
            new Vector3(386.17, 6632.66, 27.75),
            new Vector3(381.63, 6632.21, 27.78),
            new Vector3(383.93, 6628.53, 27.75),
            new Vector3(386.13, 6623.92, 27.67),
            new Vector3(381.60, 6623.49, 27.75),
            new Vector3(383.90, 6618.99, 27.71),
            new Vector3(383.80, 6608.09, 27.62),
            new Vector3(386.31, 6600.36, 27.52),
            new Vector3(381.65, 6596.80, 27.50)
        };

        private static List<Vector3> farmOneGreenhouse_THREE_PickMarkers = new List<Vector3>() // теплица 3
        {
            new Vector3(375.5163, 6642.587, 27.7331),
            new Vector3(378.5099, 6637.309, 27.79794),
            new Vector3(374.0102, 6633.422, 27.75859),
            new Vector3(376.4852, 6629.618, 27.75701),
            new Vector3(378.3688, 6626.74, 27.70975),
            new Vector3(376.6242, 6619.914, 27.73493),
            new Vector3(378.8616, 6615.024, 27.6945),
            new Vector3(373.7034, 6608.17, 27.60965),
            new Vector3(376.7436, 6603.532, 27.62383),
            new Vector3(378.9126, 6603.425, 27.61006),
            new Vector3(378.877, 6599.397, 27.61167),
            new Vector3(374.322, 6595.595, 27.59356),
            new Vector3(376.6945, 6594.178, 27.55557)
        };
        private static List<Vector3> farmOneGreenhouse_FOUR_PickMarkers = new List<Vector3>() // теплица 4
        {
            new Vector3(366.0889, 6644.111, 27.83529),
            new Vector3(368.2552, 6637.818, 27.84629),
            new Vector3(369.782, 6630.409, 27.83297),
            new Vector3(369.9022, 6625.475, 27.69963),
            new Vector3(368.1368, 6622.572, 27.79146),
            new Vector3(366.0917, 6619.811, 27.8259),
            new Vector3(367.6324, 6615.745, 27.77861),
            new Vector3(370.3022, 6611.031, 27.81285),
            new Vector3(368.2045, 6607.278, 27.7191),
            new Vector3(365.9543, 6604.618, 27.75949),
            new Vector3(367.53, 6593.406, 27.54896),
            new Vector3(369.923, 6593.696, 27.59805)
        };

        private static List<Vector3> farmOneGreenhouse_FIVE_PickMarkers = new List<Vector3>() // теплица 5
        {
            new Vector3(357.2419, 6595.438, 27.6066),
            new Vector3(358.4365, 6602.689, 27.70708),
            new Vector3(355.9274, 6610.914, 27.71772),
            new Vector3(357.4649, 6616.636, 27.67832),
            new Vector3(356.6333, 6623.84, 27.78902),
            new Vector3(354.2815, 6627.827, 27.82085),
            new Vector3(358.4911, 6635.086, 27.88318),
            new Vector3(356.4027, 6637.913, 27.8824),
            new Vector3(357.7899, 6643.545, 27.71567),
            new Vector3(355.5636, 6648.419, 27.70189)
        };

        private static List<Vector3> farmOneGreenhouse_SIX_PickMarkers = new List<Vector3>() // теплица 6
        {
            new Vector3(346.8622, 6653.903, 27.87032),
            new Vector3(348.6768, 6649.598, 27.78017),
            new Vector3(345.9515, 6644.037, 27.7008),
            new Vector3(348.2426, 6636.243, 27.7034),
            new Vector3(344.3156, 6630.751, 27.85448),
            new Vector3(348.1936, 6622.454, 27.66526),
            new Vector3(346.765, 6615.058,  27.81109),
            new Vector3(348.9263, 6608.732, 27.7707),
            new Vector3(346.2428, 6604.315, 27.7107),
            new Vector3(348.2337, 6596.896, 27.69088),
            new Vector3(346.6063, 6594.948, 27.84125)
        };

        private static List<Vector3> farmOneGreenhouse_SEVEN_PickMarkers = new List<Vector3>() // теплица 7
        {
            new Vector3(340.0319, 6596.618, 27.96909),
            new Vector3(338.6724, 6601.124, 27.79887),
            new Vector3(341.0825, 6603.153, 27.81759),
            new Vector3(338.6722, 6609.324, 27.74048),
            new Vector3(339.9794, 6614.345, 27.81946),
            new Vector3(342.0924, 6620.889, 27.8486),
            new Vector3(339.3949, 6628.181, 27.74028),
            new Vector3(341.2833, 6634.252, 27.69353),
            new Vector3(338.9107, 6641.813, 27.67465),
            new Vector3(341.3006, 6647.583, 27.75829),
            new Vector3(339.7562, 6652.127, 27.86539)
        };

        private static List<Vector3> farmOneGreenhouse_EIGHT_PickMarkers = new List<Vector3>() // теплица 8
        {
            new Vector3(332.5309, 6655.745, 27.721),
            new Vector3(334.2039, 6651.716, 27.69007),
            new Vector3(332.1431, 6646.938, 27.60129),
            new Vector3(334.0242, 6639.245, 27.65384),
            new Vector3(332.2314, 6633.862, 27.6824),
            new Vector3(334.408, 6626.521, 27.3881),
            new Vector3(332.9582, 6619.307, 27.76716),
            new Vector3(331.3477, 6612.916, 27.93123),
            new Vector3(333.938, 6606.743, 27.74311),
            new Vector3(331.9782, 6601.886, 27.81353),
            new Vector3(333.8398, 6597.376, 27.91997)
        };

        private static List<Vector3> farmOneGreenhouse_NINE_PickMarkers = new List<Vector3>() // теплица 9
        {
            new Vector3(325.4233, 6598.073, 27.00875),
            new Vector3(323.833, 6604.465, 27.81273),
            new Vector3(325.6407, 6610.372, 27.94832),
            new Vector3(324.5154, 6616.361, 27.71894),
            new Vector3(326.3145, 6621.409, 27.62288),
            new Vector3(324.3317, 6629.667, 27.65756),
            new Vector3(326.4764, 6635.217, 27.68659),
            new Vector3(323.8937, 6639.154, 27.71411),
            new Vector3(326.0826, 6645.131, 27.66325),
            new Vector3(323.3451, 6647.799, 27.87909),
            new Vector3(323.5009, 6651.084, 27.92453),
            new Vector3(325.7715, 6653.223, 27.94255)
        };

        private static List<Vector3> farmOneGreenhouse_TEN_PickMarkers = new List<Vector3>() // теплица 10
        {
            new Vector3(317.4767, 6657.277, 27.8118),
            new Vector3(319.6865, 6653.101, 27.7887),
            new Vector3(317.6208, 6650.408, 27.80974),
            new Vector3(319.3049, 6643.355, 27.76743),
            new Vector3(316.7665, 6638.804, 27.71117),
            new Vector3(318.1357, 6631.206, 27.80022),
            new Vector3(320.539, 6622.149, 27.2762),
            new Vector3(317.6141, 6616.803, 27.79947),
            new Vector3(320.0547, 6609.454, 27.06335),
            new Vector3(317.867, 6604.388, 27.07513),
            new Vector3(319.9221, 6597.376, 27.94982)
        };


        private static List<Vector3> farmOneGreenhouse_ELEVEN_PickMarkers = new List<Vector3>() // теплица 11
        {
            new Vector3(309.6131, 6595.17, 27.124571),
            new Vector3(312.3199, 6601.098, 27.81844),
            new Vector3(310.2697, 6605.695, 27.7861),
            new Vector3(311.9434, 6612.064, 27.98058),
            new Vector3(309.9967, 6619.53, 27.71928),
            new Vector3(310.5284, 6623.574, 27.61627),
            new Vector3(313.7457, 6631.336, 27.76425),
            new Vector3(312.1683, 6635.982, 27.68096),
            new Vector3(309.8883, 6643.819, 27.66416),
            new Vector3(312.4522, 6652.433, 27.73359)
        };

        #endregion 
        //
        public JobFarm()
        {
            API.onResourceStart += API_onResourceStart;
            API.onPlayerConnected += API_onPlayerConnected;
            API.onEntityEnterColShape += API_onEntityEnterColShape;
            API.onPlayerEnterVehicle += API_onPlayerEnterVehicle;
            API.onClientEventTrigger += API_onClientEventTrigger;
        }

        private void API_onClientEventTrigger(Client player, string eventName, params object[] arguments)
        {
            switch(eventName)
            {
                case "FARM_UNFREEZE":
                    API.freezePlayer(player, false);
                    break;
                case "FARM_UNFREEZE_CAR":
                    if (player.isInVehicle)
                    {
                        player.vehicle.freezePosition = false;
                    }
                    break;
                case "FARM_START_JOB":
                    startFarmJob(player);
                    break;
                case "FARM_FINISH_JOB":
                    finishFarmJob(player);
                    break;
                case "FARM_SET_CAR_TO_ACTIVE":
                    setFarmCarToActive(player);
                    break;
                case "BUY_FARM":
                    buyFarm(player);
                    break;
                case "FARM_CAR_UNLOAD":
                    unloadToWarehouse(player);
                    break;
                case "FARM_UNINVITE_FARMER":
                    unInviteToFarm(player, (string)arguments[0]);
                    break;
                case "FARM_SELL_FARM":
                    sellFarm(player);
                    break;
                case "FARM_INVITE_FARMER":
                    inviteToFarm(player, (string)arguments[0]);
                    break;
                case "FARM_GET_DATA_FROM_SERVER":
                    foreach(MarkerInfo marker in farmStorehouses)
                    {
                        if (marker.colshape.containsEntity(player))
                        {
                            farmInfo fInfo = marker.GetData("farmInfo");
                            API.triggerClientEvent(player,"RECIEVE_FARM_STOREHOUSE_DATA",fInfo.farmID, fInfo.owner, fInfo.warehouse, fInfo.productType, fInfo.productPrice, fInfo.farmPrice);
                            break;
                        }
                    }
                    //  // меню начать погрузку в машину
                    break;
                case "FARM_GET_FARMERS_LIST":
                    foreach (MarkerInfo marker in farmStorehouses)
                    {
                        if (marker.colshape.containsEntity(player))
                        {
                            farmInfo fInfo = marker.GetData("farmInfo");
                            string[] FARMERS = new string[5];
                            for (int i = 0; i < 5; i++)
                            {
                                FARMERS[i] = "";
                            }
                            for (int i = 0; i < fInfo.farmers.Count; i++)
                            {
                                FARMERS[i] = fInfo.farmers[i];
                            }
                            API.triggerClientEvent(player, "RECIEVE_FARM_FARMERS_LIST", FARMERS[0], FARMERS[1], FARMERS[2], FARMERS[3], FARMERS[4]);
                            break;
                        }
                    }
                    break;
            }
        }

        private void API_onPlayerConnected(Client player)
        {
            if (!playerEntered)
            {
                createFarmsAndMarkersAndAutos();
            }
            playerEntered = true;
        }

        private void API_onResourceStart()
        {
            lock (Global.Lock)
            {
                //
            }
        }
        private void API_onEntityEnterColShape(ColShape colshape, NetHandle entity)
        {
            if (colshape.hasData("farmMarker"))
            {
                int type = (int)API.getEntityType(entity);
                if (type == 6)
                {
                    Client player = API.getPlayerFromHandle(entity);
                    if (!player.isInVehicle)
                    {
                        farmInfo fInfo = colshape.getData("farmInfo");
                        PlayerInfo pInfo = API.getEntityData(player.handle, Constants.PlayerAccount);
                        API.triggerClientEvent(player, "FARM_SHOW_MENU_CLOTHES");
                        API.freezePlayer(player, true);
                        API.sendChatMessageToPlayer(player, string.Format("~o~Это ферма: ~g~{0}", fInfo.farmID));
                    }
                }
            }
            if (colshape.hasData("greenhouseID")) // когда сделаю меню добавлю здесь проверку что то он в машине , он фермер и машина фермераская и выйдет меню начать загрузку
            {
                int type = (int)API.getEntityType(entity);
                if (type == 6)
                {
                    Client player = API.getPlayerFromHandle(entity);
                    if (!player.isInVehicle)
                    {
                        MarkerInfo greenhouse = null;
                        foreach (MarkerInfo gh in farmGreenhouses)
                        {
                            if (gh.colshape == colshape)
                            {
                                greenhouse = gh;
                                break;
                            }
                        }
                        onEntityEntersGreenhouseMarker(player, greenhouse);
                    }
                    else // если человек в машине
                    {
                        PlayerInfo pInfo = API.getEntityData(player.handle, Constants.PlayerAccount);
                        if ((Jobs)pInfo.Job == Jobs.JOB_FARM_ONE || (Jobs)pInfo.Job == Jobs.JOB_FARM_TWO) // для каждой фермы нужно добавлять ||
                        {
                            Vehicle playerVehicle = API.getEntityFromHandle<Vehicle>(API.getPlayerVehicle(player));
                            VehInfo farmVehicle = null;
                            foreach (VehInfo fVeh in farmCars) // находим фермерская ли это машина
                            {
                                if (fVeh.Veh == playerVehicle)
                                {
                                    farmVehicle = fVeh;
                                    break;
                                }
                            }
                            if (farmVehicle != null)
                            {
                                API.triggerClientEvent(player, "FARM_SHOW_START_PICK_MENU"); // меню начать погрузку в машину
                                farmVehicle.Veh.freezePosition = true;
                            }
                        }
                    }
                }
            }
            if (colshape.hasData("pickProduct"))
            {
                int type = (int)API.getEntityType(entity);
                if (type == 6)
                {
                    Client player = API.getPlayerFromHandle(entity);
                    if (!player.isInVehicle)
                    {
                        MarkerInfo pickProduct = null;
                        foreach (MarkerInfo gh in farmPickProducts)
                        {
                            if (gh.colshape == colshape)
                            {
                                pickProduct = gh;
                                break;
                            }
                        }
                        onEntityEntersPickProductMarker(player, pickProduct);
                    }
                }
            }
            if (colshape.hasData("farmStorehouse")) // если фермер и на фермеркой машине выйдет меню чтоб разгрузить машину
            {
                int type = (int)API.getEntityType(entity);
                if (type == 6)
                {
                    Client player = API.getPlayerFromHandle(entity);
                    if (!player.isInVehicle)
                    {
                        MarkerInfo Storehouse = null;
                        foreach (MarkerInfo store in farmStorehouses)
                        {
                            if (store.colshape == colshape)
                            {
                                Storehouse = store;
                                API.sendChatMessageToPlayer(player, "~g~Введите /unloadfarm");
                                break;
                            }
                        }

                        if (colshape.containsEntity(player))
                        {
                            farmInfo fInfo = colshape.getData("farmInfo");
                            API.triggerClientEvent(player, "FARM_SHOW_STOREHOUSE_INFO_MENU");
                        }
                    }
                    else // если игрок в машине 
                    {
                        PlayerInfo pInfo = API.getEntityData(player.handle, Constants.PlayerAccount);
                        if ((Jobs)pInfo.Job == Jobs.JOB_FARM_ONE || (Jobs)pInfo.Job == Jobs.JOB_FARM_TWO) // для каждой фермы нужно добавлять ||
                        {
                            if (player.isInVehicle)
                            {
                                Vehicle playerVehicle = API.getEntityFromHandle<Vehicle>(API.getPlayerVehicle(player));
                                VehInfo farmVehicle = null;
                                foreach (VehInfo fVeh in farmCars) // находим фермерская ли это машина
                                {
                                    if (fVeh.Veh == playerVehicle)
                                    {
                                        farmVehicle = fVeh;
                                        break;
                                    }
                                }
                                if (farmVehicle != null)
                                {
                                    farmCarInfo fCarInfo = farmVehicle.GetData("farmCarInfo");
                                    farmInfo fInfo = allFarms.Find(x => x.farmID == fCarInfo.farmID);
                                    API.triggerClientEvent(player, "FARM_SHOW_STOREHOUSE_UNLOAD_MENU");
                                }
                            }
                        }
                    }
                }
            }
        }
        public void createFarmsAndMarkersAndAutos() // создаем маркера, машины блипы и т.д для всех ферм
        {
            DataTable farms = lifeRP_GM.mainClass.sqlCon.retSQLData("SELECT * FROM `farm`");
            if (farms.Rows.Count > 0)
            {
                foreach (DataRow farm in farms.Rows)
                {
                    string owner = (string)farm["owner"];
                    int farmID = (int)farm["farmID"];
                    int farmPrice = (int)farm["farmPrice"];
                    int warehouse = (int)farm["warehouse"];
                    string productType = (string)farm["productType"];
                    int productPrice = (int)farm["productPrice"];
                    string allFarmers = (string)farm["farmers"];
                    string[] farmersString = allFarmers.Split(',');
                    List<string> farmers = new List<string>();
                    foreach (string f in farmersString)
                    {
                        farmers.Add(f);
                    }
                    farmInfo fInfo = new farmInfo(owner, farmID, farmPrice, warehouse, productType, productPrice, farmers);
                    allFarms.Add(fInfo);
                    //
                }
				//TODO: сделать загрузку координат с xml
                #region farmOne
                // Marker
                MarkerInfo farmOneMarker = new MarkerInfo(1, new Vector3(416.5746, 6520.716, 26.71064), new Vector3(1, 1, 1), new Vector3(35.5, 1, 1), new Vector3(1.5, 1.5, 2), 100, 23, 182, 71, 1, 2, Jobs.JOB_FARM_ONE, 0);
                BlipInfo farmOneBlip = new BlipInfo(new Vector3(416.5746, 6520.716, 27.71064), 440, 17, true, 0, "Ферма номер: 1", Jobs.JOB_FARM_ONE);
                TLabelInfo farmOneLabel = new TLabelInfo("Раздевалка", new Vector3(416.5746, 6520.716, 27.71064), 25, 1, true);
                farmOneMarker.SetData("farmMarker", true);
                farmOneMarker.SetData("farmInfo", allFarms[0]);
                farmMarkers.Add(farmOneMarker);
                // Storehouse(склад)
                MarkerInfo farmOneStorehouse = new MarkerInfo(1, new Vector3(437.909, 6456.751, 27.44983), new Vector3(1, 1, 1), new Vector3(35.5, 1, 1), new Vector3(3, 3, 3), 100, 23, 182, 71, 4, 2, Jobs.JOB_FARM_ONE, 0);
                farmOneStorehouse.SetData("farmInfo", allFarms[0]); // here
                farmOneStorehouse.SetData("farmStorehouse", true);
                TLabelInfo farmOneStorehouseLabel = new TLabelInfo("Склад фермы", new Vector3(437.909, 6456.751, 28.44983), 25, 1, true);
                farmStorehouses.Add(farmOneStorehouse);
                // Cars
                #region farmOneCars
                VehInfo farmOneCarOne = new VehInfo((VehicleHash)1770332643, new Vector3(439.0316, 6511.595, 28.37479), new Vector3(0, 0, 92.655722), 1, 1, 100, Jobs.JOB_FARM_ONE, 0, 0);
                VehInfo farmOneCarTwo = new VehInfo((VehicleHash)1770332643, new Vector3(435.0352, 6516.271, 28.21539), new Vector3(0, 0, 89.418953), 1, 1, 100, Jobs.JOB_FARM_ONE, 0, 0);
                VehInfo farmOneCarThree = new VehInfo((VehicleHash)1770332643, new Vector3(432.9478, 6523.452, 27.72538), new Vector3(0, 0, 77.174854), 1, 1, 100, Jobs.JOB_FARM_ONE, 0, 0);
                VehInfo farmOneCarFour = new VehInfo((VehicleHash)1770332643, new Vector3(434.8398, 6527.007, 27.60105), new Vector3(0, 0, 76.824025), 1, 1, 100, Jobs.JOB_FARM_ONE, 0, 0);
                VehInfo farmOneCarFive = new VehInfo((VehicleHash)1770332643, new Vector3(435.4275, 6530.53, 27.57597), new Vector3(0, 0, 77.546926), 1, 1, 100, Jobs.JOB_FARM_ONE, 0, 0);
                VehInfo farmOneCarSix = new VehInfo((VehicleHash)1770332643, new Vector3(435.5772, 6534.391, 27.65381), new Vector3(0, 0, 79.896237), 1, 1, 100, Jobs.JOB_FARM_ONE, 0, 0);
                VehInfo farmOneCarSeven = new VehInfo((VehicleHash)1770332643, new Vector3(436.124, 6537.97, 27.71378), new Vector3(0, 0, 78.067888), 1, 1, 100, Jobs.JOB_FARM_ONE, 0, 0);
                VehInfo farmOneCarEight = new VehInfo((VehicleHash)1770332643, new Vector3(436.6291, 6541.459, 27.64757), new Vector3(0, 0, 78.789739), 1, 1, 100, Jobs.JOB_FARM_ONE, 0, 0);
                VehInfo farmOneCarNine = new VehInfo((VehicleHash)1770332643, new Vector3(414.7422, 6540.393, 27.38413), new Vector3(0, 0, -96.0560710), 1, 1, 100, Jobs.JOB_FARM_ONE, 0, 0);
                farmCarInfo fInfoCarOne = new farmCarInfo(farmOneCarOne, 1, 0, 0, false);
                farmCarInfo fInfoCarTwo = new farmCarInfo(farmOneCarTwo, 1, 0, 0, false);
                farmCarInfo fInfoCarThree = new farmCarInfo(farmOneCarThree, 1, 0, 0, false);
                farmCarInfo fInfoCarFour = new farmCarInfo(farmOneCarFour, 1, 0, 0, false);
                farmCarInfo fInfoCarFive = new farmCarInfo(farmOneCarFive, 1, 0, 0, false);
                farmCarInfo fInfoCarSix = new farmCarInfo(farmOneCarSix, 1, 0, 0, false);
                farmCarInfo fInfoCarSeven = new farmCarInfo(farmOneCarSeven, 1, 0, 0, false);
                farmCarInfo fInfoCarEight = new farmCarInfo(farmOneCarEight, 1, 0, 0, false);
                farmCarInfo fInfoCarNine = new farmCarInfo(farmOneCarNine, 1, 0, 0, false);
                farmOneCarOne.SetData("farmCarInfo", fInfoCarOne);
                farmOneCarTwo.SetData("farmCarInfo", fInfoCarTwo);
                farmOneCarThree.SetData("farmCarInfo", fInfoCarThree);
                farmOneCarFour.SetData("farmCarInfo", fInfoCarFour);
                farmOneCarFive.SetData("farmCarInfo", fInfoCarFive);
                farmOneCarSix.SetData("farmCarInfo", fInfoCarSix);
                farmOneCarSeven.SetData("farmCarInfo", fInfoCarSeven);
                farmOneCarEight.SetData("farmCarInfo", fInfoCarEight);
                farmOneCarNine.SetData("farmCarInfo", fInfoCarNine);
                farmCars.Add(farmOneCarOne);
                farmCars.Add(farmOneCarTwo);
                farmCars.Add(farmOneCarThree);
                farmCars.Add(farmOneCarFour);
                farmCars.Add(farmOneCarFive);
                farmCars.Add(farmOneCarSix);
                farmCars.Add(farmOneCarSeven);
                farmCars.Add(farmOneCarEight);
                farmCars.Add(farmOneCarNine);
                farmCarInfos.Add(fInfoCarOne);
                farmCarInfos.Add(fInfoCarTwo);
                farmCarInfos.Add(fInfoCarThree);
                farmCarInfos.Add(fInfoCarFour);
                farmCarInfos.Add(fInfoCarFive);
                farmCarInfos.Add(fInfoCarSix);
                farmCarInfos.Add(fInfoCarSeven);
                farmCarInfos.Add(fInfoCarEight);
                farmCarInfos.Add(fInfoCarNine);
                #endregion
                #region farmOneGreenhouses
                // Greenhouse 1
                MarkerInfo farmOneGreenhouseOne = new MarkerInfo(1, new Vector3(390.983, 6652.66, 27.7532), new Vector3(1, 1, 1), new Vector3(35.5, 1, 1), new Vector3(3, 3, 3), 100, 23, 182, 71, 4, 2, Jobs.JOB_FARM_ONE, 0);
                farmGreenhouses.Add(farmOneGreenhouseOne);
                farmOneGreenhouseOne.SetData("farmID", 1);
                farmOneGreenhouseOne.SetData("greenhouseID", 1);
                farmOneGreenhouseOne.SetData("isWorking", false);
                // Greenhouse 2
                MarkerInfo farmOneGreenhouseTwo = new MarkerInfo(1, new Vector3(383.99, 6652.69, 27.25), new Vector3(1, 1, 1), new Vector3(35.5, 1, 1), new Vector3(3, 3, 3), 100, 23, 182, 71, 4, 2, Jobs.JOB_FARM_ONE, 0);
                farmGreenhouses.Add(farmOneGreenhouseTwo);
                farmOneGreenhouseTwo.SetData("farmID", 1);
                farmOneGreenhouseTwo.SetData("greenhouseID", 2);
                farmOneGreenhouseTwo.SetData("isWorking", false);
                // Greenhouse 3
                MarkerInfo farmOneGreenhouseThree = new MarkerInfo(1, new Vector3(376.6873, 6652.451, 27.73034), new Vector3(1, 1, 1), new Vector3(35.5, 1, 1), new Vector3(3, 3, 3), 100, 23, 182, 71, 4, 2, Jobs.JOB_FARM_ONE, 0);
                farmGreenhouses.Add(farmOneGreenhouseThree);
                farmOneGreenhouseThree.SetData("farmID", 1);
                farmOneGreenhouseThree.SetData("greenhouseID", 3);
                farmOneGreenhouseThree.SetData("isWorking", false);
                // Greenhouse 4
                MarkerInfo farmOneGreenhouseFour = new MarkerInfo(1, new Vector3(368.6357, 6656.128, 27.70676), new Vector3(1, 1, 1), new Vector3(35.5, 1, 1), new Vector3(3, 3, 3), 100, 23, 182, 71, 4, 2, Jobs.JOB_FARM_ONE, 0);
                farmGreenhouses.Add(farmOneGreenhouseFour);
                farmOneGreenhouseFour.SetData("farmID", 1);
                farmOneGreenhouseFour.SetData("greenhouseID", 4);
                farmOneGreenhouseFour.SetData("isWorking", false);
                // Greenhouse 5
                MarkerInfo farmOneGreenhouseFive = new MarkerInfo(1, new Vector3(356.8371, 6656.318, 27.83904), new Vector3(1, 1, 1), new Vector3(35.5, 1, 1), new Vector3(3, 3, 3), 100, 23, 182, 71, 4, 2, Jobs.JOB_FARM_ONE, 0);
                farmGreenhouses.Add(farmOneGreenhouseFive);
                farmOneGreenhouseFive.SetData("farmID", 1);
                farmOneGreenhouseFive.SetData("greenhouseID", 5);
                farmOneGreenhouseFive.SetData("isWorking", false);
                // Greenhouse 6
                MarkerInfo farmOneGreenhouseSix = new MarkerInfo(1, new Vector3(347.3059, 6660.722, 27.78365), new Vector3(1, 1, 1), new Vector3(35.5, 1, 1), new Vector3(3, 3, 3), 100, 23, 182, 71, 4, 2, Jobs.JOB_FARM_ONE, 0);
                farmGreenhouses.Add(farmOneGreenhouseSix);
                farmOneGreenhouseSix.SetData("farmID", 1);
                farmOneGreenhouseSix.SetData("greenhouseID", 6);
                farmOneGreenhouseSix.SetData("isWorking", false);
                // Greenhouse 7
                MarkerInfo farmOneGreenhouseSeven = new MarkerInfo(1, new Vector3(340.6801, 6660.906, 27.86593), new Vector3(1, 1, 1), new Vector3(35.5, 1, 1), new Vector3(3, 3, 3), 100, 23, 182, 71, 4, 2, Jobs.JOB_FARM_ONE, 0);
                farmGreenhouses.Add(farmOneGreenhouseSeven);
                farmOneGreenhouseSeven.SetData("farmID", 1);
                farmOneGreenhouseSeven.SetData("greenhouseID", 7);
                farmOneGreenhouseSeven.SetData("isWorking", false);
                // Greenhouse 8
                MarkerInfo farmOneGreenhouseEight = new MarkerInfo(1, new Vector3(333.3888, 6660.998, 27.874), new Vector3(1, 1, 1), new Vector3(35.5, 1, 1), new Vector3(3, 3, 3), 100, 23, 182, 71, 4, 2, Jobs.JOB_FARM_ONE, 0);
                farmGreenhouses.Add(farmOneGreenhouseEight);
                farmOneGreenhouseEight.SetData("farmID", 1);
                farmOneGreenhouseEight.SetData("greenhouseID", 8);
                farmOneGreenhouseEight.SetData("isWorking", false);
                // Greenhouse 9
                MarkerInfo farmOneGreenhouseNine = new MarkerInfo(1, new Vector3(324.7018, 6660.35, 27.86045), new Vector3(1, 1, 1), new Vector3(35.5, 1, 1), new Vector3(3, 3, 3), 100, 23, 182, 71, 4, 2, Jobs.JOB_FARM_ONE, 0);
                farmGreenhouses.Add(farmOneGreenhouseNine);
                farmOneGreenhouseNine.SetData("farmID", 1);
                farmOneGreenhouseNine.SetData("greenhouseID", 9);
                farmOneGreenhouseNine.SetData("isWorking", false);
                // Greenhouse 10
                MarkerInfo farmOneGreenhouseTen = new MarkerInfo(1, new Vector3(318.2462, 6659.787, 27.84273), new Vector3(1, 1, 1), new Vector3(35.5, 1, 1), new Vector3(3, 3, 3), 100, 23, 182, 71, 4, 2, Jobs.JOB_FARM_ONE, 0);
                farmGreenhouses.Add(farmOneGreenhouseTen);
                farmOneGreenhouseTen.SetData("farmID", 1);
                farmOneGreenhouseTen.SetData("greenhouseID", 10);
                farmOneGreenhouseTen.SetData("isWorking", false);
                // Greenhouse 11
                MarkerInfo farmOneGreenhouseEleven = new MarkerInfo(1, new Vector3(311.8827, 6660.539, 27.8551), new Vector3(1, 1, 1), new Vector3(35.5, 1, 1), new Vector3(3, 3, 3), 100, 23, 182, 71, 4, 2, Jobs.JOB_FARM_ONE, 0);
                farmGreenhouses.Add(farmOneGreenhouseEleven);
                farmOneGreenhouseEleven.SetData("farmID", 1);
                farmOneGreenhouseEleven.SetData("greenhouseID", 11);
                farmOneGreenhouseEleven.SetData("isWorking", false);
                #endregion
                #endregion
                //
            }
        }

        public void startFarmJob(Client player)
        {
            MarkerInfo markerInfo = null;
            bool markerWasFounded = false;
            foreach (MarkerInfo marker in farmMarkers)
            {
                if (marker.colshape.containsEntity(player.handle))
                {
                    markerInfo = marker;
                    markerWasFounded = true;
                    break;
                }
            }
            if (markerWasFounded)
            {
                PlayerInfo pInfo = API.getEntityData(player.handle, Constants.PlayerAccount);
                if ((Jobs)pInfo.Job == Jobs.NULL)
                {
                    farmInfo fInfo = markerInfo.GetData("farmInfo");
                    switch (fInfo.farmID)
                    {
                        case 1:
                            pInfo.Job = (int)Jobs.JOB_FARM_ONE;
                            break;
                    }
                    if (pInfo.sex) // мужик
                    {
                        if (fInfo.owner == player.name) // владелец
                        {
                            API.setPlayerClothes(player, 11, 24, 0);
                            API.setPlayerClothes(player, 4, 23, 0);
                            API.setPlayerAccessory(player, 0, 13, 0);
                        }
                        else if (fInfo.farmers.Contains(player.name)) // если фермер
                        {
                            API.setPlayerClothes(player, 11, 43, 0);
                            API.setPlayerClothes(player, 4, 0, 0);
                            API.setPlayerAccessory(player, 0, 12, 0);
                        }
                        else // простой рабочий
                        {
                            API.setPlayerClothes(player, 11, 0, 0);
                            API.setPlayerClothes(player, 4, 46, 0);
                            API.setPlayerAccessory(player, 0, 83, 0);
                            
                        }
                    }
                    else // девушка
                    {
                        API.setPlayerClothes(player, 11, 0, 3);
                        API.setPlayerClothes(player, 4, 46, 0);
                        API.setPlayerAccessory(player, 0, 83, 0);
                    }
                    API.setEntityData(player, "farmID", fInfo.farmID);
                    API.sendChatMessageToPlayer(player, string.Format("~y~[Server] ~g~Вы устроились на работу. Ферма №: {0}", fInfo.farmID.ToString()));
                }
                else
                    API.sendChatMessageToPlayer(player, "~y~[Server] ~r~Вы уже где-то работаете.");
            }
        }

        public void finishFarmJob(Client player)
        {
            MarkerInfo markerInfo = null;
            bool markerWasFounded = false;
            foreach (MarkerInfo marker in farmMarkers)
            {
                if (marker.colshape.containsEntity(player.handle))
                {
                    markerInfo = marker;
                    markerWasFounded = true;
                    break;
                }
            }
            if (markerWasFounded)
            {
                PlayerInfo pInfo = API.getEntityData(player.handle, Constants.PlayerAccount);
                if ((Jobs)pInfo.Job == Jobs.JOB_FARM_ONE || (Jobs)pInfo.Job == Jobs.JOB_FARM_TWO)
                {
                    pInfo.Job = (int)Jobs.NULL;
                    API.sendChatMessageToPlayer(player, "~y~[Server] ~b~Вы уволились с работы фермера");
                    API.sendChatMessageToPlayer(player, string.Format("~y~[Server] ~o~Ваша заработная плата: ~g~{0}", pInfo.jobMoney));
                    API.sendNotificationToPlayer(player, string.Format("~g~ +{0}", pInfo.jobMoney));
                    pInfo.money += pInfo.jobMoney;
                    pInfo.jobMoney = 0;
                    API.setPlayerDefaultClothes(player);
                    API.clearPlayerAccessory(player, 0);
                    if (API.hasEntityData(player, "hasEnteredGreenhouseMarker") && API.getEntityData(player, "hasEnteredGreenhouseMarker") == true)
                    {
                        if (API.hasEntityData(player, "hasPickedProduct") && API.getEntityData(player, "hasPickedProduct") == false)
                        {
                            MarkerInfo pickMarker = API.getEntityData(player, "pickMarker");
                            deletePickMarker(pickMarker);
                        }
                    }
                    if (API.hasEntityData(player, "hasFarmPickObject") && API.getEntityData(player, "hasFarmPickObject") == true)
                    {
                        var obj = API.getEntityData(player, "farmPickObject");
                        API.detachEntity(obj);
                        API.deleteEntity(obj);
                    }
                    API.setEntityData(player, "hasEnteredGreenhouseMarker", false);
                }
                else
                    API.sendChatMessageToPlayer(player, "~y~[Server] ~r~Вы здесь не работаете");
            }
        }

        public void setFarmCarToActive(Client player) // поставить машину для сбора урожая
        {
            PlayerInfo pInfo = API.getEntityData(player.handle, Constants.PlayerAccount);
            if ((Jobs)pInfo.Job == Jobs.JOB_FARM_ONE || (Jobs)pInfo.Job == Jobs.JOB_FARM_TWO) // для каждой фермы нужно добавлять ||
            {
                if (player.isInVehicle)
                {
                    Vehicle playerVehicle = API.getEntityFromHandle<Vehicle>(API.getPlayerVehicle(player));
                    VehInfo farmVehicle = null;
                    foreach (VehInfo fVeh in farmCars) // находим фермерская ли это машина
                    {
                        if (fVeh.Veh == playerVehicle)
                        {
                            farmVehicle = fVeh;
                            break;
                        }
                    }
                    if (farmVehicle != null)
                    {
                        farmCarInfo fCarInfo = farmVehicle.GetData("farmCarInfo");
                        farmInfo fInfo = allFarms.Find(x => x.farmID == fCarInfo.farmID);
                        if (!fInfo.farmers.Contains(player.name))
                        {
                            API.sendChatMessageToPlayer(player, "~y~[Server] ~r~Вы не фермер этой фермы");
                            return;
                        }
                        if (fCarInfo.productCount < 1000 && fCarInfo.isWorking == false)
                        {
                            MarkerInfo greenhouse = null;
                            foreach (MarkerInfo gh in farmGreenhouses) // смотрим стоит ли он рядом с теплицей (на маркере)
                            {
                                if (gh.colshape.containsEntity(farmVehicle.Veh))
                                {
                                    greenhouse = gh;
                                    break;
                                }
                            }
                            if (greenhouse != null)
                            {
                                int farmID = greenhouse.GetData("farmID");
                                if (farmID == fCarInfo.farmID)
                                {
                                    if (greenhouse.GetData("isWorking") == false)
                                    {
                                        TLabelInfo label = new TLabelInfo(string.Format("Идет загрузка: {0} / 1000", fCarInfo.productCount), new Vector3(player.position.X, player.position.Y - 3, player.position.Z), 25, 1, true);
                                        label.SetData("farmID", farmID);
                                        label.SetData("farmCar", farmVehicle);
                                        greenhouse.SetData("farmCar", farmVehicle);
                                        greenhouse.SetData("isWorking", true);
                                        farmGreenhouseLabels.Add(label);
                                        fCarInfo.isWorking = true;
                                        fCarInfo.greenhouseID = greenhouse.GetData("greenhouseID");
                                        //farmVehicle.Veh.rotation = new Vector3(0, 0, 0.39);
                                        API.warpPlayerOutOfVehicle(player);
                                    }
                                    else
                                        API.sendChatMessageToPlayer(player, "~y~[Server] ~r~На этой теплице уже идет сбор урожая");
                                }
                            }
                            API.sendChatMessageToPlayer(player, "~y~[Server] ~r~Вы не у теплицы");
                        }
                        else
                            API.sendChatMessageToPlayer(player, "~y~[Server] ~o~Автомобиль заполнен продуктами");
                    }
                    else return;
                }
            }
        }
        private void API_onPlayerEnterVehicle(Client player, NetHandle vehicle)
        {
            if (API.shared.getEntitySyncedData(vehicle, Constants.VehJob) == (int)Jobs.JOB_FARM_ONE || API.shared.getEntitySyncedData(vehicle, Constants.VehJob) == (int)Jobs.JOB_FARM_TWO) // для каждого добавлять
            {
                PlayerInfo pInfo = API.getEntityData(player.handle, Constants.PlayerAccount);
                if ((Jobs)pInfo.Job == Jobs.JOB_FARM_ONE || (Jobs)pInfo.Job == Jobs.JOB_FARM_TWO) // для каждой фермы нужно добавлять ||
                {
                    farmCarInfo fCarInfo = API.getEntityData(vehicle, "farmCarInfo");
                    farmInfo fInfo = allFarms.Find(x => x.farmID == fCarInfo.farmID);
                    if (fInfo.farmers.Contains(player.name))
                    {
                        if (fCarInfo.isWorking == true)
                        {
                            VehInfo farmCar = null;
                            TLabelInfo greenhouseLabel = null;
                            bool carWasFounded = false;
                            foreach (TLabelInfo label in farmGreenhouseLabels)// находим label от машины
                            {
                                farmCar = label.GetData("farmCar");
                                if (farmCar.Veh == vehicle)
                                {
                                    carWasFounded = true;
                                    greenhouseLabel = label;
                                    break;
                                }
                            }
                            if (carWasFounded)
                            {
                                MarkerInfo greenhouseMarker = null;
                                foreach (MarkerInfo gh in farmGreenhouses)
                                {
                                    VehInfo ghCar = gh.GetData("farmCar");
                                    if (farmCar == ghCar)
                                    {
                                        greenhouseMarker = gh;
                                        break;
                                    }
                                }
                                // удаляем клиентский маркер если он есть
                                if (API.hasEntityData(player, "hasEnteredGreenhouseMarker") && API.getEntityData(player, "hasEnteredGreenhouseMarker") == true)
                                {
                                    if (API.hasEntityData(player, "hasPickedProduct") && API.getEntityData(player, "hasPickedProduct") == false)
                                    {
                                        MarkerInfo pickMarker = API.getEntityData(player, "pickMarker");
                                        deletePickMarker(pickMarker);
                                    }
                                } // удаляем объект если он есть
                                if (API.hasEntityData(player, "hasFarmPickObject") && API.getEntityData(player, "hasFarmPickObject") == true)
                                {
                                    var obj = API.getEntityData(player, "farmPickObject");
                                    API.detachEntity(obj);
                                    API.deleteEntity(obj);
                                }
                                API.setEntityData(player, "hasEnteredGreenhouseMarker", false);
                                API.setEntityData(player, "hasPickedProduct", false);
                                //
                                fCarInfo.isWorking = false;
                                greenhouseMarker.SetData("isWorking", false);
                                greenhouseLabel.DeleteTextLabel();
                                farmGreenhouseLabels.Remove(greenhouseLabel);
                                API.sendChatMessageToPlayer(player, string.Format("~y~[Server] ~b~Количество продуктов в машине: ~g~{0}", fCarInfo.productCount));
                            }
                        }
                    }
                    else
                    {
                        API.sendChatMessageToPlayer(player, "~y~[Server] ~r~Вы не фермер этой фермы");
                        API.warpPlayerOutOfVehicle(player);
                    }
                }
                else
                {
                    API.sendChatMessageToPlayer(player, "~y~[Server] ~r~Вы не фермер этой фермы");
                    API.warpPlayerOutOfVehicle(player);
                }

            }
        }
        private void onEntityEntersGreenhouseMarker(Client player, MarkerInfo greenHouseMarker)
        {
            if (!player.isInVehicle)
            {
                PlayerInfo pInfo = API.getEntityData(player.handle, Constants.PlayerAccount);
                if ((Jobs)pInfo.Job == Jobs.JOB_FARM_ONE || (Jobs)pInfo.Job == Jobs.JOB_FARM_TWO) // для каждой фермы нужно добавлять || (случай)
                {
                    int playerFarmID = API.getEntityData(player, "farmID");
                    int markerFarmID = greenHouseMarker.GetData("farmID");
                    VehInfo farmCar = greenHouseMarker.GetData("farmCar");
                    if (playerFarmID == markerFarmID)
                    {
                        bool isWorking = greenHouseMarker.GetData("isWorking");
                        if (isWorking)
                        {
                            if (!API.hasEntityData(player, "hasEnteredGreenhouseMarker") || API.getEntityData(player, "hasEnteredGreenhouseMarker") == false)
                            {
                                farmInfo fInfo = allFarms.Find(x => x.farmID == playerFarmID);
                                farmCarInfo fCarInfo = farmCarInfos.Find(x => x.car == farmCar);
                                if (fCarInfo.productCount < 1000)
                                {
                                    API.setEntityData(player, "hasEnteredGreenhouseMarker", true);
                                    API.setEntityData(player, "hasPickedProduct", false);
                                    createPickMarker(player, fInfo, fCarInfo);
                                }
                                else
                                    API.sendChatMessageToPlayer(player, "~y~[Server] ~r~Автомобиль заполнен продуктами.");

                            }
                            else
                            {
                                if (API.hasEntityData(player, "hasEnteredGreenhouseMarker") && API.getEntityData(player, "hasEnteredGreenhouseMarker") == true)
                                {
                                    if (API.hasEntityData(player, "hasPickedProduct") && API.getEntityData(player, "hasPickedProduct") == true)
                                    {
                                        farmInfo fInfo = allFarms.Find(x => x.farmID == playerFarmID);
                                        farmCarInfo fCarInfo = farmCarInfos.Find(x => x.car == farmCar);
                                        if (fCarInfo.productCount < 1000)
                                        {
                                            fCarInfo.productCount += 10;
                                            TLabelInfo greenHouseLabel = null;
                                            foreach (TLabelInfo label in farmGreenhouseLabels)
                                            {
                                                VehInfo labelCar = label.GetData("farmCar");
                                                if (labelCar == farmCar)
                                                {
                                                    greenHouseLabel = label;
                                                    break;
                                                }
                                            }
                                            pInfo.jobMoney += fInfo.productPrice;
                                            greenHouseLabel.tLabel.text = string.Format("Идет загрузка: {0} / 1000", fCarInfo.productCount);
                                            API.sendNotificationToPlayer(player, string.Format("~b~Заработная плата: ~g~{0}", pInfo.jobMoney));
                                            API.setEntityData(player, "hasPickedProduct", false);
                                            API.setEntityData(player, "hasFarmPickObject", false);
                                            MarkerInfo pickMarker = API.getEntityData(player, "pickMarker");
                                            var obj = API.getEntityData(player, "farmPickObject");
                                            API.detachEntity(obj);
                                            API.deleteEntity(obj);
                                            deletePickMarker(pickMarker);
                                            createPickMarker(player, fInfo, fCarInfo);
                                        }
                                        else
                                            API.sendChatMessageToPlayer(player, "~y~[Server] ~r~Автомобиль заполнен продуктами.");
                                    }
                                    else
                                        API.sendChatMessageToPlayer(player, "~y~[Server] ~b~Для начала соберите урожай");
                                }
                            }
                        }
                        if (!isWorking)
                        {
                            API.sendChatMessageToPlayer(player, "~y~[Server] ~r~На этой теплице сбор урожая невозможен");
                        }
                    }
                    else
                        API.sendChatMessageToPlayer(player, "~y~[Server] ~r~Вы не работаете на этой ферме");
                }
                else
                    API.sendChatMessageToPlayer(player, "~y~[Server] ~r~Вы не работаете на ферме");
            }
        }

        private void onEntityEntersPickProductMarker(Client player, MarkerInfo pickProductMarker)
        {
            if (!player.isInVehicle)
            {
                PlayerInfo pInfo = API.getEntityData(player.handle, Constants.PlayerAccount);
                if ((Jobs)pInfo.Job == Jobs.JOB_FARM_ONE || (Jobs)pInfo.Job == Jobs.JOB_FARM_TWO)
                {
                    API.playPlayerAnimation(player, 0, "amb@world_human_gardener_plant@male@enter", "enter");
                    timerFirstAnim = new System.Timers.Timer(2000);
                    timerFirstAnim.AutoReset = true;
                    timerFirstAnim.Enabled = true;
                    timerFirstAnim.Elapsed += (sender, e) => OnTimedEventFirstTimer(sender, e, player);
                    API.freezePlayer(player, true);
                    timerFirstAnim.Start();

                    //
                    timerSecondAnim = new System.Timers.Timer(2000);
                    timerSecondAnim.AutoReset = true;
                    timerSecondAnim.Enabled = true;
                    timerSecondAnim.Elapsed += (sender, e) => OnTimedEventSecondTimer(sender, e, player, pickProductMarker);
                }
            }
        }
        private void OnTimedEventFirstTimer(System.Object source, ElapsedEventArgs e, Client player)
        {
            API.playPlayerAnimation(player, 0, "amb@world_human_gardener_plant@male@base", "base");
            timerSecondAnim.Start();
            timerFirstAnim.Stop();
        }
        private void OnTimedEventSecondTimer(System.Object source, ElapsedEventArgs e, Client player, MarkerInfo pickProductMarker)
        {
            farmInfo fInfo = pickProductMarker.GetData("farmInfo");
            GrandTheftMultiplayer.Server.Elements.Object obj = null;
            switch (fInfo.farmID)
            {
                case 1: // farm 1
                    obj = API.createObject(-1027805354, new Vector3(-774.7673, -2657.982, 13.0375), new Vector3(0, 0, -68.67301)); // гриб
                    break;
            }
            API.attachEntityToEntity(obj, player, "36029", new Vector3(), new Vector3(-70, 2, 0));
            deletePickMarker(pickProductMarker);
            API.setEntityData(player, "hasPickedProduct", true);
            API.setEntityData(player, "farmPickObject", obj);
            API.setEntityData(player, "hasFarmPickObject", true);
            API.freezePlayer(player, false);
            timerSecondAnim.Stop();
        }
        private void createPickMarker(Client player, farmInfo fInfo, farmCarInfo fCarInfo)
        {
            Random rnd = new Random();
            int index = 0;
            MarkerInfo pickProduct = null;
            switch (fCarInfo.farmID)
            {
                case 1: // farm 1
                    switch (fCarInfo.greenhouseID)
                    {
                        case 1: // greenhouse 1
                            index = rnd.Next(farmOneGreenhouse_ONE_PickMarkers.Count);
                            pickProduct = new MarkerInfo(player, 1, farmOneGreenhouse_ONE_PickMarkers[index], new Vector3(1, 1, 1), new Vector3(35.5, 1, 1), new Vector3(1.5, 1.5, 2), 100, 255, 0, 0, 1, 2, Jobs.JOB_FARM_ONE, 0);
                            break;
                        case 2: // greenhouse 2
                            index = rnd.Next(farmOneGreenhouse_TWO_PickMarkers.Count);
                            pickProduct = new MarkerInfo(player, 1, farmOneGreenhouse_TWO_PickMarkers[index], new Vector3(1, 1, 1), new Vector3(35.5, 1, 1), new Vector3(1.5, 1.5, 2), 100, 255, 0, 0, 1, 2, Jobs.JOB_FARM_ONE, 0);
                            break;
                        case 3: // greenhouse 3
                            index = rnd.Next(farmOneGreenhouse_THREE_PickMarkers.Count);
                            pickProduct = new MarkerInfo(player, 1, farmOneGreenhouse_THREE_PickMarkers[index], new Vector3(1, 1, 1), new Vector3(35.5, 1, 1), new Vector3(1.5, 1.5, 2), 100, 255, 0, 0, 1, 2, Jobs.JOB_FARM_ONE, 0);
                            break;
                        case 4: // greenhouse 4
                            index = rnd.Next(farmOneGreenhouse_FOUR_PickMarkers.Count);
                            pickProduct = new MarkerInfo(player, 1, farmOneGreenhouse_FOUR_PickMarkers[index], new Vector3(1, 1, 1), new Vector3(35.5, 1, 1), new Vector3(1.5, 1.5, 2), 100, 255, 0, 0, 1, 2, Jobs.JOB_FARM_ONE, 0);
                            break;
                        case 5: // greenhouse 5
                            index = rnd.Next(farmOneGreenhouse_FIVE_PickMarkers.Count);
                            pickProduct = new MarkerInfo(player, 1, farmOneGreenhouse_FIVE_PickMarkers[index], new Vector3(1, 1, 1), new Vector3(35.5, 1, 1), new Vector3(1.5, 1.5, 2), 100, 255, 0, 0, 1, 2, Jobs.JOB_FARM_ONE, 0);
                            break;
                        case 6: // greenhouse 6
                            index = rnd.Next(farmOneGreenhouse_SIX_PickMarkers.Count);
                            pickProduct = new MarkerInfo(player, 1, farmOneGreenhouse_SIX_PickMarkers[index], new Vector3(1, 1, 1), new Vector3(35.5, 1, 1), new Vector3(1.5, 1.5, 2), 100, 255, 0, 0, 1, 2, Jobs.JOB_FARM_ONE, 0);
                            break;
                        case 7: // greenhouse 7
                            index = rnd.Next(farmOneGreenhouse_SEVEN_PickMarkers.Count);
                            pickProduct = new MarkerInfo(player, 1, farmOneGreenhouse_SEVEN_PickMarkers[index], new Vector3(1, 1, 1), new Vector3(35.5, 1, 1), new Vector3(1.5, 1.5, 2), 100, 255, 0, 0, 1, 2, Jobs.JOB_FARM_ONE, 0);
                            break;
                        case 8: // greenhouse 8
                            index = rnd.Next(farmOneGreenhouse_EIGHT_PickMarkers.Count);
                            pickProduct = new MarkerInfo(player, 1, farmOneGreenhouse_EIGHT_PickMarkers[index], new Vector3(1, 1, 1), new Vector3(35.5, 1, 1), new Vector3(1.5, 1.5, 2), 100, 255, 0, 0, 1, 2, Jobs.JOB_FARM_ONE, 0);
                            break;
                        case 9: // greenhouse 9
                            index = rnd.Next(farmOneGreenhouse_NINE_PickMarkers.Count);
                            pickProduct = new MarkerInfo(player, 1, farmOneGreenhouse_NINE_PickMarkers[index], new Vector3(1, 1, 1), new Vector3(35.5, 1, 1), new Vector3(1.5, 1.5, 2), 100, 255, 0, 0, 1, 2, Jobs.JOB_FARM_ONE, 0);
                            break;
                        case 10: // greenhouse 10
                            index = rnd.Next(farmOneGreenhouse_TEN_PickMarkers.Count);
                            pickProduct = new MarkerInfo(player, 1, farmOneGreenhouse_TEN_PickMarkers[index], new Vector3(1, 1, 1), new Vector3(35.5, 1, 1), new Vector3(1.5, 1.5, 2), 100, 255, 0, 0, 1, 2, Jobs.JOB_FARM_ONE, 0);
                            break;
                        case 11: // greenhouse 11
                            index = rnd.Next(farmOneGreenhouse_ELEVEN_PickMarkers.Count);
                            pickProduct = new MarkerInfo(player, 1, farmOneGreenhouse_ELEVEN_PickMarkers[index], new Vector3(1, 1, 1), new Vector3(35.5, 1, 1), new Vector3(1.5, 1.5, 2), 100, 255, 0, 0, 1, 2, Jobs.JOB_FARM_ONE, 0);
                            break;
                    }
                    break;
                case 2: // farm 2
                    break;
            }
            pickProduct.SetData("farmInfo", fInfo);
            pickProduct.SetData("farmCarInfo", fCarInfo);
            pickProduct.SetData("pickProduct", true);
            API.setEntityData(player, "pickMarker", pickProduct);
            farmPickProducts.Add(pickProduct);
        }

        private void deletePickMarker(MarkerInfo markerInfo)
        {
            markerInfo.DeleteMarker();
            markerInfo.colshape.resetData("farmInfo");
            markerInfo.colshape.resetData("farmCarInfo");
            farmPickProducts.Remove(farmPickProducts.Find(x => x.marker == markerInfo.marker));
        }

        private void unloadToWarehouse(Client player) // функция разгрузки урожая на главный склад
        {
            PlayerInfo pInfo = API.getEntityData(player.handle, Constants.PlayerAccount);
            if ((Jobs)pInfo.Job == Jobs.JOB_FARM_ONE || (Jobs)pInfo.Job == Jobs.JOB_FARM_TWO) // для каждой фермы нужно добавлять ||
            {
                if (player.isInVehicle)
                {
                    Vehicle playerVehicle = API.getEntityFromHandle<Vehicle>(API.getPlayerVehicle(player));
                    VehInfo farmVehicle = null;
                    foreach (VehInfo fVeh in farmCars) // находим фермерская ли это машина
                    {
                        if (fVeh.Veh == playerVehicle)
                        {
                            farmVehicle = fVeh;
                            break;
                        }
                    }
                    if (farmVehicle != null)
                    {
                        farmCarInfo fCarInfo = farmVehicle.GetData("farmCarInfo");
                        farmInfo fInfo = allFarms.Find(x => x.farmID == fCarInfo.farmID);
                        if (!fInfo.farmers.Contains(player.name))
                        {
                            API.sendChatMessageToPlayer(player, "~y~[Server] ~r~Вы не фермер этой фермы");
                            return;
                        }
                        MarkerInfo Storehouse = null;
                        foreach (MarkerInfo store in farmStorehouses) // смотрим стоит ли он рядом с складом (на маркере)
                        {
                            if (store.colshape.containsEntity(farmVehicle.Veh))
                            {
                                Storehouse = store;
                                break;
                            }
                        }
                        if (Storehouse != null)
                        {
                            if (fCarInfo.productCount > 0)
                            {
                                if (fInfo.warehouse + fCarInfo.productCount <= maxWarehouse)
                                {
                                    fInfo.warehouse += fCarInfo.productCount;
                                    fCarInfo.productCount = 0;
                                    fInfo.updateFarm();
                                    API.sendChatMessageToPlayer(player, string.Format("~y~[Server] ~b~ Вы разгрузили продукты, на складе: ~g~{0}", fInfo.warehouse));
                                }
                                else
                                    API.sendChatMessageToPlayer(player, "~y~[Server] ~r~На складе недостаточно места");
                            }
                            else
                                API.sendChatMessageToPlayer(player, "~y~[Server] ~r~В автомобиле нет продуктов");
                        }
                        else
                            API.sendChatMessageToPlayer(player, "~y~[Server] ~r~Вы не у склада");
                    }
                    else return;
                }
            }
        }
        private void inviteToFarm(Client owner, string playerName)
        {
            bool isOwner = false; // фермер ли тот кто вводит команду?
            farmInfo fInfo = null;
            foreach(farmInfo Info in allFarms)
            {
                if (Info.owner == owner.name)
                {
                    isOwner = true;
                    fInfo = Info;
                    break;
                }
            }
            if (isOwner)
            {
                API.sendChatMessageToPlayer(owner, playerName);
                Client player = lifeRP_GM.mainClass.findPlayer(owner, playerName);
                if (player != null)
                {
                    bool playerIsInRadius = false;
                    
                    List<Client> nearPlayers = API.getPlayersInRadiusOfPlayer(3, owner);
                    foreach (Client nearPlayer in nearPlayers)
                    {
                        if (nearPlayer == player)
                        {
                            playerIsInRadius = true;
                            break;
                        }
                    }
                    if (playerIsInRadius)
                    {
                        bool playerIsFree = true;
                        foreach (farmInfo Info in allFarms)
                        {
                            if (Info.farmers.Contains(player.name))
                            {
                                playerIsFree = false;
                                break;
                            }
                        }
                        if (playerIsFree)
                        {
                            API.sendChatMessageToPlayer(player, string.Format("~y~[Server]~b~Вы были приглашены на должность фермера, ферма №: ~g~{0}", fInfo.farmID));
                            API.setEntityData(player, "invitedFarm", fInfo);
                            // триггер на меню принять или отменить
                        }
                        else
                            API.sendChatMessageToPlayer(owner, "~y~[Server] ~r~ Этот игрок уже фермер");
                    }
                    else
                        API.sendChatMessageToPlayer(owner, "~y~[Server] ~r~Данный игрок находится далеко от вас");
                }
            }
        }
        private void acceptInviteToFarm(Client player)
        {
            farmInfo fInfo = API.getEntityData(player, "invitedFarm");
            API.sendChatMessageToPlayer(player, string.Format("~y~[Server] ~b~Вы ~r~приняли ~b~приглашение, ферма №: ~g~{0}", fInfo.farmID));
            fInfo.farmers.Add(player.name);
            fInfo.updateFarm();
            // триггер на закрытие меню
        }
        private void unAcceptInviteToFarm(Client player)
        {
            farmInfo fInfo = API.getEntityData(player, "invitedFarm");
            API.sendChatMessageToPlayer(player, string.Format("~y~[Server] ~b~Вы ~r~отклонили ~b~приглашение, ферма №: ~g~{0}", fInfo.farmID));
            // триггер на закрытие меню
        }
        private void unInviteToFarm(Client owner, string playerName)
        {
            bool isOwner = false; // фермер ли тот кто вводит команду?
            farmInfo fInfo = null;
            foreach (farmInfo Info in allFarms)
            {
                if (Info.owner == owner.name)
                {
                    isOwner = true;
                    fInfo = Info;
                    break;
                }
            }
            if (isOwner)
            {
                if (fInfo.farmers.Contains(playerName))
                {
                    fInfo.farmers.Remove(playerName);
                    fInfo.updateFarm();
                    API.sendChatMessageToPlayer(owner, string.Format("~y~[Server] ~r~ Вы уволили фермера: ~b~{0}", playerName));
                }

            }
        }
        private void buyFarm(Client player)
        {
            bool isOwner = false; // фермер ли тот кто вводит команду?
            foreach (farmInfo Info in allFarms)
            {
                if (Info.owner == player.name)
                {
                    isOwner = true;
                    break;
                }
            }
            if (!isOwner) // если он не владелец фермы.
            {
                MarkerInfo farmMarker = null;
                foreach(MarkerInfo marker in farmStorehouses)
                {
                    if (marker.colshape.containsEntity(player))
                    {
                        farmMarker = marker;
                        break;
                    }
                }
                if (farmMarker != null)
                {
                    farmInfo fInfo = farmMarker.GetData("farmInfo");
                    if (fInfo.owner == "")
                    {
                        PlayerInfo pInfo = API.getEntityData(player.handle, Constants.PlayerAccount);
                        if (pInfo.money < fInfo.farmPrice)
                        {
                            API.sendChatMessageToPlayer(player, "~y~[Server] ~r~У вас недостаточно денег для покупки фермы");
                            return;
                        }
                        // списали деньги
                        pInfo.money -= fInfo.farmPrice;
                        // поставили фермером покупателя
                        fInfo.owner = player.name;
                        fInfo.farmers.Clear(); // удалили всех фермеров
                        fInfo.farmers.Add(player.name);
                        fInfo.updateFarm();
                        API.sendChatMessageToPlayer(player, string.Format("~y~[Server] ~b~Вы приобрели ферму. Ферма №: ~g~{0}.",fInfo.farmID));

                    }
                    else
                        API.sendChatMessageToPlayer(player, "~y~[Server] ~r~Эта ферма уже кем-то куплена.");
                }
            }
            else
                API.sendChatMessageToPlayer(player, "~y~[Server] ~r~Вы уже владеете одной из ферм");
        }
        private void sellFarm(Client player)
        {
            bool isOwner = false; // фермер ли тот кто вводит команду?
            foreach (farmInfo Info in allFarms)
            {
                if (Info.owner == player.name)
                {
                    isOwner = true;
                    break;
                }
            }
            if (isOwner) // если он владелец фермы.
            {
                MarkerInfo farmMarker = null;
                foreach (MarkerInfo marker in farmStorehouses)
                {
                    if (marker.colshape.containsEntity(player))
                    {
                        farmMarker = marker;
                        break;
                    }
                }
                if (farmMarker != null)
                {
                    farmInfo fInfo = farmMarker.GetData("farmInfo");
                    if (fInfo.owner == player.name)
                    {
                        PlayerInfo pInfo = API.getEntityData(player.handle, Constants.PlayerAccount);
                        pInfo.money += fInfo.farmPrice;
                        fInfo.owner = "";
                        fInfo.farmers.Clear();
                        fInfo.updateFarm();
                        API.sendChatMessageToPlayer(player, "~y~[Server] ~g~Вы успешно продали свою ферму");
                    }
                    else
                        API.sendChatMessageToPlayer(player, "~y~[Server] ~r~Вы не владелец этой фермы");
                }
            }
        }
        private void openfarmMenu(Client player)
        {
            bool isOwner = false; // фермер ли тот кто вводит команду?
            foreach (farmInfo Info in allFarms)
            {
                if (Info.owner == player.name)
                {
                    isOwner = true;
                    break;
                }
            }
            if (isOwner) // если он владелец фермы.
            {
                MarkerInfo farmMarker = null;
                foreach (MarkerInfo marker in farmStorehouses)
                {
                    if (marker.colshape.containsEntity(player))
                    {
                        farmMarker = marker;
                        break;
                    }
                }
                if (farmMarker != null)
                {
                    farmInfo fInfo = farmMarker.GetData("farmInfo");
                    if (fInfo.owner == player.name)
                    {
                        API.triggerClientEvent(player,"FARM_SHOW_OWNER_MENU");
                    }
                }
                else
                    API.sendChatMessageToPlayer(player, "~y~[Server] ~r~Вы должны находится у склада фермы");
            }
        }
        [Command("goo4")]
        public void goo4(Client player)
        {
            API.setEntityPosition(player, new Vector3(420.87, 6515, 27.71));
        }
        [Command("getsex")]
        public void getsex(Client player)
        {
            PlayerInfo pInfo = API.getEntityData(player.handle, Constants.PlayerAccount);
            API.sendChatMessageToPlayer(player, pInfo.sex.ToString());
        }
        [Command("farmers")]
        public void farmers(Client player)
        {
            farmInfo fInfo = allFarms[0];
            foreach (string farmer in fInfo.farmers)
            {
                API.sendChatMessageToPlayer(player, farmer);
            }
        }
        [Command("save")]
        public void save(Client player, string text)
        {
            Vector3 pos = API.getEntityPosition(player);
            Vector3 rot = API.getEntityRotation(player);
            File.AppendAllText("coords.txt", string.Format("{0}, {1}, {2}, {3}", pos.X, pos.Y, pos.Z, rot.Z) + text);
            File.AppendAllText("coords.txt", Environment.NewLine);
        }
        [Command("invitefarm")]
        public void invitefarm(Client player, string a)
        {
            inviteToFarm(player, a);
        }
        [Command("acceptfarm")]
        public void acceptfarm(Client player)
        {
            acceptInviteToFarm(player);
        }
        [Command("unacceptfarm")]
        public void unacceptfarm(Client player)
        {
            unAcceptInviteToFarm(player);
        }
        [Command("buyfarm")]
        public void buyfarm_CMD(Client player)
        {
            buyFarm(player);
        }
        [Command("sellfarm")]
        public void sellfarm_CMD(Client player)
        {
            sellFarm(player);
        }
        [Command("farmmenu")]
        public void farmmenu_CMD(Client player)
        {
            openfarmMenu(player);
        }
    }

}