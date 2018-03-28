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
//------------------------------------------------------------------------------------------------------------------------------------------------------------------------
/* Чтобы добавить новый АвтоСалон(класс) нужно:
 Записать входной маркер, координаты появления машины после покупки, координаты roration при появлении после покупки,
 координаты появления машины в автосалоне, так же rotation машины. координаты камеры и rotation камеры.
 Создать List на подобии autoHashes_Muscle где будут хранится возможноные покупки в определенном классе
 В фнукцию createAutoBuyMarkersForEachClass вписать координаты маркера. Маркеру дать 3 даты. Сделать все как в примере с Muscle 
 В spawnCarInAutoBuy добавить в два switch случаи для нового автосалона (по примеру с Muscle)
 В acceptBuyCar добавить в switch для нового автосалона (по примеру с Muscle) а также в другие функции где есть switch с классами.
 В функции: setClassDatas,getExitCoords getRespawnCoords getRespawnRotation, getMaxCarIndex, getDefaultParkCoords, getDefaultParkRotation
 getAutoBuyCameraView, getAutoBuyCameraRotation, getAutoBuyCarPosition, getAutoBuyCarRotation добавить в switch случай для нового автосалона*/

namespace _4_lifeRP
{
    class AutoBuy : Script
    {
        class autoInfo // Класс для представления машины в виртуальном мире
        {
            public Client owner;
            public VehicleHash autoName;
            public int color1 = 0;
            public int color2 = 0;
            public double spawnX = -1;
            public double spawnY = -1;
            public double spawnZ = -1;
            public double rotation = -1;
            public VehInfo ownersVehicle;
            public autoInfo(Client itsOwner, VehicleHash itsAutoName, int itsColor1, int itsColor2, double itsSpawnX, double itsSpawnY, double itsSpawnZ, double itsRotation, VehInfo itsOwnersVehicle)
            {
                owner = itsOwner;
                autoName = itsAutoName;
                color1 = itsColor1;
                color2 = itsColor2;
                spawnX = itsSpawnX;
                spawnY = itsSpawnY;
                spawnZ = itsSpawnZ;
                rotation = itsRotation;
                ownersVehicle = itsOwnersVehicle;
            }
        }
        class sellCarInfo // Класс для представлении информации о машине в виртуальном мире
        {
            public VehicleHash vHash;
            public int price;
            public sellCarInfo(VehicleHash itsVHash, int itsPrice)
            {
                vHash = itsVHash;
                price = itsPrice;
            }
        }
        private System.Timers.Timer testDriveTimer;
        //
        private const int maxVehiclesNumber = 10; // Максимальное кол-во машин у одного человека.
        private static List<autoInfo> allAutoInfo = new List<autoInfo>(); // храним здесь инфо о автомобилях
        private static List<VehInfo> vehicles = new List<VehInfo>(); // храним здесь автомобили.
        private static List<int> autoBuyDimensions = new List<int>(); // Dimension где будут появлятся машины во время покупки.
        private static List<MarkerInfo> autoBuyEnterMarkers = new List<MarkerInfo>(); // маркеры входа
        // List где храним хэшы машин которые продаются в автосалоне.
		
		//TODO: сделать загрузку хэшей из xml
        #region Muscle
        private static List<sellCarInfo> autoHashes_Muscle = new List<sellCarInfo>()
        {
         new sellCarInfo  ((VehicleHash)(-1205801634),  18000),
         new sellCarInfo  ((VehicleHash)(-682211828),   26000),
         new sellCarInfo  ((VehicleHash)(349605904),    22000),
         new sellCarInfo  ((VehicleHash)(-1361687965),  42000),
         new sellCarInfo  ((VehicleHash)(80636076),     50000),
         new sellCarInfo  ((VehicleHash)(-915704871),   55000),
         new sellCarInfo  ((VehicleHash)(723973206),    30000),
         new sellCarInfo  ((VehicleHash)(-326143852),   60000),
         new sellCarInfo  ((VehicleHash)(-2119578145),  25000),
         new sellCarInfo  ((VehicleHash)(-1790546981),  42300),
         new sellCarInfo  ((VehicleHash)(-2039755226),  50000),
         new sellCarInfo  ((VehicleHash)(-1800170043),  32500),
         new sellCarInfo  ((VehicleHash)(349315417),    55300),
         new sellCarInfo  ((VehicleHash)(37348240),     38400),
         new sellCarInfo  ((VehicleHash)(2068293287),   65000),
         new sellCarInfo  ((VehicleHash)(525509695),    44000),
         new sellCarInfo  ((VehicleHash)(1896491931),   52800),
         new sellCarInfo  ((VehicleHash)(-1943285540),  60000),
         new sellCarInfo  ((VehicleHash)(-2095439403),  26000),
         new sellCarInfo  ((VehicleHash)(1507916787),   25000),
         new sellCarInfo  ((VehicleHash)(-667151410),   32800),
         new sellCarInfo  ((VehicleHash)(-589178377),   39800),
         new sellCarInfo  ((VehicleHash)(-227741703),   28400),
         new sellCarInfo  ((VehicleHash)(941494461),    42800),
         new sellCarInfo  ((VehicleHash)(-1685021548),  27900),
         new sellCarInfo  ((VehicleHash)(223258115),    68400),
         new sellCarInfo  ((VehicleHash)(729783779),    25100),
         new sellCarInfo  ((VehicleHash)(833469436),    32700),
         new sellCarInfo  ((VehicleHash)(1119641113),   49300),
         new sellCarInfo  ((VehicleHash)(1923400478),   30900),
         new sellCarInfo  ((VehicleHash)(-401643538),   32000),
         new sellCarInfo  ((VehicleHash)(972671128),    38000),
         new sellCarInfo  ((VehicleHash)(-825837129),   15800),
         new sellCarInfo  ((VehicleHash)(-498054846),   16000),
         new sellCarInfo  ((VehicleHash)(-899509638),   18300),
         new sellCarInfo  ((VehicleHash)(16646064),     35400),
         new sellCarInfo  ((VehicleHash)(2006667053),   48900),
         new sellCarInfo  ((VehicleHash)(523724515),    16000)
        };
        #endregion
        #region OffRoad
        private static List<sellCarInfo> autoHashes_OffRoad = new List<sellCarInfo>()
        {
            new sellCarInfo ((VehicleHash)(1126868326),   18300),
            new sellCarInfo ((VehicleHash)(-349601129),   14800),
            new sellCarInfo ((VehicleHash)(-2128233223),  25000),
            new sellCarInfo ((VehicleHash)(-48031959),    35000),
            new sellCarInfo ((VehicleHash)(-1269889662),  13000),
            new sellCarInfo ((VehicleHash)(-1590337689),  15000),
            new sellCarInfo ((VehicleHash)(-1435919434),  30800),
            new sellCarInfo ((VehicleHash)(-1479664699),  32800),
            new sellCarInfo ((VehicleHash)(1770332643),   36000),
            new sellCarInfo ((VehicleHash)(92612664),     13800),
            new sellCarInfo ((VehicleHash)(914654722),    44000),
            new sellCarInfo ((VehicleHash)(-2064372143),  50000),
            new sellCarInfo ((VehicleHash)(1645267888),   55800),
            new sellCarInfo ((VehicleHash)(-1207771834),  60200),
            new sellCarInfo ((VehicleHash)(-2045594037),  65200),
            new sellCarInfo ((VehicleHash)(-1189015600),  75000),
            new sellCarInfo ((VehicleHash)(989381445),    80000),
            new sellCarInfo ((VehicleHash)(101905590),    85200),
            new sellCarInfo ((VehicleHash)(-663299102),   89000),
        };
        #endregion
        #region Lowriders
        private static List<sellCarInfo> autoHashes_Lowriders = new List<sellCarInfo>()
        {
            new sellCarInfo ((VehicleHash)(-1013450936),   45000),
            new sellCarInfo ((VehicleHash)(-1361687965),  42000),
            new sellCarInfo ((VehicleHash)(-1790546981),  44300),
            new sellCarInfo ((VehicleHash)(-2039755226),  50000),
            new sellCarInfo ((VehicleHash)(1896491931),   52800),
            new sellCarInfo ((VehicleHash)(223258115),    68400),
            new sellCarInfo ((VehicleHash)(1119641113),   50300),
            new sellCarInfo ((VehicleHash)(-899509638),   48300),
            new sellCarInfo ((VehicleHash)(2006667053),   48900)
        };
        #endregion
        //
        public AutoBuy() 
        {
            API.onClientEventTrigger += onClientEvent;
            API.onResourceStart += API_onResourceStart;
            API.onEntityEnterColShape += API_onMarkerEnter;
            API.onPlayerDisconnected += API_onPlayerDisconnected;
        }
        private void API_onMarkerEnter(ColShape colshape, NetHandle entity)
        {
            if (colshape.hasData("autoBuyMarker"))
            {
                int type = (int)API.getEntityType(entity);
                if (type == 6)
                {
                    Client player = API.getPlayerFromHandle(entity);
                    if (!player.isInVehicle)
                    {
                        if (!API.hasEntityData(player, "AUTOBUY_IS_BUYING_CAR") || API.getEntityData(player, "AUTOBUY_IS_BUYING_CAR") == false)
                        {
                            string autoClass = colshape.getData("autoBuyMarker_CLASS");
                            API.sendChatMessageToPlayer(player, string.Format("~y~[Server] ~o~Автосалон: {0}", autoClass));
                            API.triggerClientEvent(player, "AutoBuyEnterMenu");
                            API.freezePlayer(player, true);                            
                        }
                    }
                }
            }
        }

        private void API_onResourceStart()
        {
            lock (Global.Lock)
            {
                createAutoBuyMarkersForEachClass();
            }
        }
        private void API_onPlayerDisconnected(Client player, string reason)
        {
            try
            {
                // удаляем личные машины игрока.
                deletePlayerAutos(player);
                // если игрок вышел во время покупки в автосалоне
                if (API.hasEntityData(player, "AUTOBUY_IS_BUYING_CAR") && API.getEntityData(player, "AUTOBUY_IS_BUYING_CAR") == true)
                {
                    int carDimension = (int)API.getEntityData(player, "AUTOBUY_CURRENT_PLAYER_CAR_DIMENSION");
                    autoBuyDimensions.Remove(autoBuyDimensions.Find(x => x == carDimension)); // удалили из листа дименшененов (нужно для нахождения свободного)
                    VehInfo vehicle = API.getEntityData(player, "AUTOBUY_CREATED_CAR");
                    vehicle.DeleteVehicle();
                }
            }
            catch(Exception ex)
            {
                ServerPlayers.playersClass.sendChatMessageToDevelopers("~r~[DevChat] ошибка в disconnect" + ex.Message + " Имя: " + player.name);
            }
        }
        private void onClientEvent(Client player, string eventName, params object[] arguments)
        {
            switch (eventName)
            { 
                case Constants.CharStartPlay: // Начало игры после выбора персонажа 
                    lifeRP_GM.mainClass.Stream(1500, () =>
                    {
                        createPlayerAutos(player);
                    });
                    break;
                case Constants.LoginingOut:
                    try
                    {
                        // удаляем личные машины игрока.
                        deletePlayerAutos(player);
                        // если игрок вышел во время покупки в автосалоне
                        if (API.hasEntityData(player, "AUTOBUY_IS_BUYING_CAR") && API.getEntityData(player, "AUTOBUY_IS_BUYING_CAR") == true)
                        {
                            int carDimension = (int)API.getEntityData(player, "AUTOBUY_CURRENT_PLAYER_CAR_DIMENSION");
                            autoBuyDimensions.Remove(autoBuyDimensions.Find(x => x == carDimension)); // удалили из листа дименшененов (нужно для нахождения свободного)
                            VehInfo vehicle = API.getEntityData(player, "AUTOBUY_CREATED_CAR");
                            vehicle.DeleteVehicle();
                        }
                    }
                    catch (Exception ex)
                    {
                        ServerPlayers.playersClass.sendChatMessageToDevelopers("~r~[DevChat] ошибка в disconnect" + ex.Message + " Имя: " + player.name);
                    }
                    break;
                case "AUTOBUY_UNFREEZE":
                    API.freezePlayer(player, false);
                    break;
                case "AUTOBUY_BUYCAR":
                    buycar(player);
                    break;
                case "AUTOBUY_EXIT":
                    exitBuyCar(player);
                    break;
                case "AUTOBUY_NEXT_CAR":
                    changeVehicleTo_NEXT(player);
                    break;
                case "AUTOBUY_PREVIOUS_CAR":
                    changeVehicleTo_PREVIOUS(player);
                    break;
                case "AUTOBUY_NEXT_COLOR_1":
                    changeVehicleColor_ONE_To_NEXT(player);
                    break;
                case "AUTOBUY_NEXT_COLOR_2":
                    changeVehicleColor_TWO_To_NEXT(player);
                    break;
                case "AUTOBUY_PREVIOUS_COLOR_1":
                    changeVehicleColor_ONE_To_PREVIOUS(player);
                    break;
                case "AUTOBUY_PREVIOUS_COLOR_2":
                    changeVehicleColor_TWO_To_PREVIOUS(player);
                    break;
                case "AUTOBUY_ACCEPT_BUYCAR":
                    acceptBuyCar(player);
                    break;
                case "AUTOBUY_TEST_DRIVE":
                    testDrive(player);
                    break;
                case "AUTOBUY_TEST_DRIVE_TIMER":
                    stopTestDrive(player);
                    break;
                case "AUTOBUY_CAR_AND_PRICE":
                    string carClass = API.getEntityData(player, "AUTOBUY_CLASS");
                    VehInfo tempVehicle = API.getEntityData(player, "AUTOBUY_CREATED_CAR"); // получаем выбранную машину
                    sellCarInfo carInfo = null;
                    switch (carClass)
                    {
                        case "Muscle":
                            carInfo = autoHashes_Muscle.Find(x => x.vHash == tempVehicle.vHash);
                            break;
                        case "OffRoad":
                            carInfo = autoHashes_OffRoad.Find(x => x.vHash == tempVehicle.vHash);
                            break;
                        case "Lowriders":
                            carInfo = autoHashes_Lowriders.Find(x => x.vHash == tempVehicle.vHash);
                            break;
                    }
                    API.triggerClientEvent(player, "RECIEVE_CAR_AND_PRICE", tempVehicle.Veh.displayName, carInfo.price);
                    break;
            }
        }
        // Функция получения свободного dimension от 1000+ (минимального)
        public int getFreeDimension1000()
        {
            int freeDimension = 100;
            foreach (int dimension in autoBuyDimensions)
            {
                if (freeDimension == dimension)
                {
                    freeDimension++;
                    continue;
                }
                else break;
            }
            return freeDimension;
        }
        // Функция создания машин при входе в игру.
        public void createPlayerAutos(Client player)
        {
            DataTable playerAutos = lifeRP_GM.mainClass.sqlCon.retSQLData(string.Format("SELECT * FROM `autobuy` WHERE `playerName` = '{0}'", player.name));
            if (playerAutos.Rows.Count > 0)
            {
                foreach (DataRow playerAuto in playerAutos.Rows)
                {
                    string hash = (string)playerAuto["autoName"];
                    int h = Convert.ToInt32(hash);
                    VehicleHash vehHash = (VehicleHash)h;
                    int color1 = (int)playerAuto["color1"];
                    int color2 = (int)playerAuto["color2"];
                    string X = (playerAuto["spawnX"]).ToString();
                    string Y = (playerAuto["spawnY"]).ToString();
                    string Z = (playerAuto["spawnZ"]).ToString();
                    string R = (playerAuto["rotation"]).ToString();
                    double spawnX = Convert.ToDouble(X);
                    double spawnY = Convert.ToDouble(Y);
                    double spawnZ = Convert.ToDouble(Z);
                    double rotation = Convert.ToDouble(R);
                    VehInfo vehicle = new VehInfo(vehHash, new Vector3(spawnX, spawnY, spawnZ), new Vector3(0, 0, rotation), color1, color2, 100);
                    autoInfo aInfo = new autoInfo(player, vehHash, color1, color2, spawnX, spawnY, spawnZ, rotation, vehicle);
                    API.setEntityData(vehicle.Veh, "autoInfo", aInfo);
                    allAutoInfo.Add(aInfo);
                    vehicles.Add(vehicle);
                    API.sendChatMessageToPlayer(player, "~g~Тачка " + vehicle.Veh.displayName + " ждет тебя");
                }
            }
        }
        // Функция удаления машин игрока при выходе из игры.
        public void deletePlayerAutos(Client player)
        {
            List<autoInfo> autoInfos = allAutoInfo.FindAll(x => x.owner == player); // НАДО ПРОВЕРИТЬ -----------------------------------------------------------------------------
            foreach(autoInfo aInfo in autoInfos)
            {
                // удаляем всю существующую информацию об этой машине и саму машину
                aInfo.ownersVehicle.DeleteVehicle();
                vehicles.Remove(vehicles.Find(x => x.Veh == aInfo.ownersVehicle.Veh));
                allAutoInfo.Remove(allAutoInfo.Find(x => x.owner == player));
            }
        }
        // Глобальный спавн машин. (Все функции смены машины/цвета вызывают эту функцию)
        public void spawnCarInAutoBuy(Client player)
        {
            string carClass = API.getEntityData(player, "AUTOBUY_CLASS");
            int index = (int)API.getEntityData(player, "AUTOBUY_CAR_INDEX_NUMBER");
            int carDimension = (int)API.getEntityData(player, "AUTOBUY_CURRENT_PLAYER_CAR_DIMENSION");
            int color1 = (int)API.getEntityData(player, "AUTOBUY_CAR_COLOR_ONE"); // получаем color1
            int color2 = (int)API.getEntityData(player, "AUTOBUY_CAR_COLOR_TWO"); // получаем color2
            VehInfo vehicle = null; // эту машину будем спавнить как показательную
            VehInfo veh = null; // это прошлая машина
            if (API.hasEntityData(player, "AUTOBUY_CREATED_CAR"))
            {
                vehicle = API.getEntityData(player, "AUTOBUY_CREATED_CAR"); // получаем прошлую заспавненную машину, если существует
                vehicle.DeleteVehicle(); // удаляем её
            }
            Vector3 autoBuyCarPosition = getAutoBuyCarPosition(carClass);
            Vector3 autoBuyCarRotation = getAutoBuyCarRotation(carClass);
            switch (carClass)
            {
                case "Muscle":
                    veh = new VehInfo(autoHashes_Muscle[index].vHash, autoBuyCarPosition, autoBuyCarRotation, color1, color2, 100, Jobs.NULL, 300, carDimension);
                    break;
                case "OffRoad":
                    veh = new VehInfo(autoHashes_OffRoad[index].vHash, autoBuyCarPosition, autoBuyCarRotation, color1, color2, 100, Jobs.NULL, 300, carDimension);
                    break;
                case "Lowriders":
                    veh = new VehInfo(autoHashes_Lowriders[index].vHash, autoBuyCarPosition, autoBuyCarRotation, color1, color2, 100, Jobs.NULL, 300, carDimension);
                    break;
            }
            API.setEntityData(player, "AUTOBUY_CREATED_CAR", veh);
        }
        #region Смена машины, цвета
            // Спавн следующей машины
        public void changeVehicleTo_NEXT(Client player)
        {
            string carClass = API.getEntityData(player, "AUTOBUY_CLASS");
            int maxIndex = getMaxCarIndex(carClass); // получаем максимальное кол-во машин в определенном классе.
            int index = (int)API.getEntityData(player, "AUTOBUY_CAR_INDEX_NUMBER");
            if (index + 1 < maxIndex) // если в диапазоне ЗДЕСЬ НУЖНО ЕЩЕ ПОСМОТРЕТЬ // 40 стремно
            {
                index++;
                API.setEntityData(player, "AUTOBUY_CAR_INDEX_NUMBER", index);
                spawnCarInAutoBuy(player);
            }
            else
                API.sendChatMessageToPlayer(player, "~o~Машин то дальше нету..");
        }
        // Спавн предыдущей машины
        public void changeVehicleTo_PREVIOUS(Client player)
        {
            int index = (int)API.getEntityData(player, "AUTOBUY_CAR_INDEX_NUMBER");
            if (index - 1 >= 0) // если в диапазоне
            {
                index--;
                API.setEntityData(player, "AUTOBUY_CAR_INDEX_NUMBER", index);
                spawnCarInAutoBuy(player);
            }
            else
                API.sendChatMessageToPlayer(player, "~o~Машин то дальше нету..");
        }
        // Смена COLOR 1
        public void changeVehicleColor_ONE_To_NEXT(Client player)
        {
            int color1 = (int)API.getEntityData(player, "AUTOBUY_CAR_COLOR_ONE"); // получаем color1
            if (color1 + 1 <= 159) // если в диапазоне
            {
                color1++;
                API.setEntityData(player, "AUTOBUY_CAR_COLOR_ONE", color1); // запомнили новый color1
                spawnCarInAutoBuy(player);
            }
            else
                API.sendChatMessageToPlayer(player, "~o~Цвет 1 вне предела");
        }
        public void changeVehicleColor_ONE_To_PREVIOUS(Client player)
        {
            int color1 = (int)API.getEntityData(player, "AUTOBUY_CAR_COLOR_ONE"); // получаем color1
            if (color1 - 1 >= 0) // если в диапазоне
            {
                color1--;
                API.setEntityData(player, "AUTOBUY_CAR_COLOR_ONE", color1); // запомнили новый color1
                spawnCarInAutoBuy(player);
            }
            else
                API.sendChatMessageToPlayer(player, "~o~Цвет 1 вне предела");
        }
        // Смена COLOR 2
        public void changeVehicleColor_TWO_To_NEXT(Client player)
        {
            int color2 = (int)API.getEntityData(player, "AUTOBUY_CAR_COLOR_TWO"); // получаем color2
            if (color2 + 1 <= 159) // если в диапазоне
            {
                color2++;
                API.setEntityData(player, "AUTOBUY_CAR_COLOR_TWO", color2); // запомнили новый color2
                spawnCarInAutoBuy(player);
            }
            else
                API.sendChatMessageToPlayer(player, "~o~Цвет 2 вне предела");
        }
        public void changeVehicleColor_TWO_To_PREVIOUS(Client player)
        {
            int color2 = (int)API.getEntityData(player, "AUTOBUY_CAR_COLOR_TWO"); // получаем color2
            if (color2 - 1 >= 0) // если в диапазоне
            {
                color2--;
                API.setEntityData(player, "AUTOBUY_CAR_COLOR_TWO", color2); // запомнили новый color2
                spawnCarInAutoBuy(player);
            }
            else
                API.sendChatMessageToPlayer(player, "~o~Цвет 2 вне предела");
        }
        #endregion
        // Функция начала покупки машины
        public void buycar(Client player)
        {
            if (!API.hasEntityData(player, "AUTOBUY_IS_BUYING_CAR") || API.getEntityData(player, "AUTOBUY_IS_BUYING_CAR") == false)
            {
                MarkerInfo markerInfo = null;
                bool markerWasFounded = false;
                foreach (MarkerInfo marker in autoBuyEnterMarkers)
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
                    string autoClass = markerInfo.GetData("autoBuyMarker_CLASS"); // берем с маркера класс автосалона
                    setClassDatas(player, autoClass); // этой функцией назначаем класс автосалона где находится игрок
                    Vector3 autoBuyCameraView = getAutoBuyCameraView(autoClass);
                    Vector3 autoBuyCameraRotation = getAutoBuyCameraRotation(autoClass);
                    Vector3 autoBuyCarPosition = getAutoBuyCarPosition(autoClass);
                    // получили свободный дименшен от 1000+
                    int freeDimension = getFreeDimension1000();
                    API.setEntityDimension(player, freeDimension); // посадили его в дименшен
                    API.setEntityPosition(player, new Vector3(autoBuyCarPosition.X + 10, autoBuyCarPosition.Y, autoBuyCarPosition.Z)); // спавним его в автосалоне (недалеко от машины)
                    API.setEntityData(player, "AUTOBUY_IS_BUYING_CAR", true);
                    API.setEntityData(player, "AUTOBUY_CURRENT_PLAYER_CAR_DIMENSION", freeDimension); // запомнили дименшен игрока
                    API.setEntityData(player, "AUTOBUY_CAR_INDEX_NUMBER", 0); // запомнили первый хеш машины (индекс)
                    API.setEntityData(player, "AUTOBUY_CAR_COLOR_ONE", 0); // запомнили color1
                    API.setEntityData(player, "AUTOBUY_CAR_COLOR_TWO", 0); // запомнили color2
                    API.setEntityInvincible(player, true); // сделали невидимкой
                    API.freezePlayer(player, true); // заморозили на месте
                    API.triggerClientEvent(player, "SET_CAMERA_FOR_CAR_BUY", autoBuyCameraView, autoBuyCameraRotation); // создаем камеру
                    spawnCarInAutoBuy(player); // функция создания машины в автосалоне для определенного человека. (различается dimension у покупателей)
                    autoBuyDimensions.Add(freeDimension); // добавляем в List дименшенов
                }
                else
                    API.sendChatMessageToPlayer(player, "Нет маркера");
            }
        }
        // Функция отмены покупки машины
        public void exitBuyCar(Client player)
        {
           // если он в автосалоне в данный момент
           if (API.hasEntityData(player, "AUTOBUY_IS_BUYING_CAR") && API.getEntityData(player, "AUTOBUY_IS_BUYING_CAR") == true)
           {
                try
                {
                    string autoClass = API.getEntityData(player, "AUTOBUY_CLASS"); // получили в каком из автослаонов он
                    Vector3 position = getExitCoords(autoClass); // по автосалону получили координаты выхода.
                    API.setEntityData(player, "AUTOBUY_IS_BUYING_CAR", false);
                    API.triggerClientEvent(player, "SET_CAMERA_FOR_CAR_BUY_TO_NULL");
                    API.setEntityPosition(player, position);
                    API.setEntityDimension(player, 0);
                    API.freezePlayer(player, false);
                    int carDimension = (int)API.getEntityData(player, "AUTOBUY_CURRENT_PLAYER_CAR_DIMENSION");
                    autoBuyDimensions.Remove(autoBuyDimensions.Find(x => x == carDimension)); // удалили из листа дименшененов (нужно для нахождения свободного)
                    VehInfo vehicle = API.getEntityData(player, "AUTOBUY_CREATED_CAR"); // удаляем временную машину созданную в автосалоне
                    vehicle.DeleteVehicle();
                    API.sendChatMessageToPlayer(player, "~y~[Server] ~r~Вы отменили покупку машины.");
                }
                catch (Exception ex)
                {
                    ServerPlayers.playersClass.sendChatMessageToDevelopers("~r~[DevChat] ошибка в exitBuy" + ex.Message + " Имя: " + player.name);
                }
           }
        }
        // Функция подтверждения покупки данной машины.
        public void acceptBuyCar(Client player)
        {
            // если он в автосалоне в данный момент
            if (API.hasEntityData(player, "AUTOBUY_IS_BUYING_CAR") && API.getEntityData(player, "AUTOBUY_IS_BUYING_CAR") == true)
            {
                // проверка на максимальное кол-во покупки машин
                int playerCarsNumber = 0;
                DataTable playerAutos = lifeRP_GM.mainClass.sqlCon.retSQLData(string.Format("SELECT * FROM `autobuy` WHERE `playerName` = '{0}'", player.name));
                if (playerAutos.Rows.Count > 0)
                {
                    playerCarsNumber = playerAutos.Rows.Count;
                }     
                if (playerCarsNumber >= maxVehiclesNumber)
                {
                    API.sendChatMessageToPlayer(player, "~y~[Server] ~r~У вас максимальное кол-во купленных машин.");
                    return;
                }
                //
                string carClass = API.getEntityData(player, "AUTOBUY_CLASS");
                VehInfo tempVehicle = API.getEntityData(player, "AUTOBUY_CREATED_CAR"); // получаем выбранную машину
                sellCarInfo carInfo = null;
                switch (carClass)
                {
                    case "Muscle":
                        carInfo = autoHashes_Muscle.Find(x => x.vHash == tempVehicle.vHash);
                        break;
                    case "OffRoad":
                        carInfo = autoHashes_OffRoad.Find(x => x.vHash == tempVehicle.vHash);
                        break;
                    case "Lowriders":
                        carInfo = autoHashes_Lowriders.Find(x => x.vHash == tempVehicle.vHash);
                        break;
                }
                
                PlayerInfo pInfo = API.getEntityData(player.handle, Constants.PlayerAccount);
                //if (pInfo.money < carInfo.price)
                //{
                //    API.sendChatMessageToPlayer(player, "~y~[Server] ~rНедостаточно денег для покупки автомобиля.");
                //    return;
                //}
                Vector3 position = getRespawnCoords(carClass);
                Vector3 rotation = getRespawnRotation(carClass);
                int color1 = (int)API.getEntityData(player, "AUTOBUY_CAR_COLOR_ONE"); // получаем color1
                int color2 = (int)API.getEntityData(player, "AUTOBUY_CAR_COLOR_TWO"); // получаем color2
                VehInfo boughtVehicle = new VehInfo(tempVehicle.vHash, position, rotation, color1, color2, 100); // создаем машину
                API.setPlayerIntoVehicle(player, boughtVehicle.Veh, -1); // сажаем на вод.место     
                API.setEntityDimension(player, 0); // дименшен на 0
                API.freezePlayer(player, false); // фриз на false
                API.triggerClientEvent(player, "SET_CAMERA_FOR_CAR_BUY_TO_NULL"); // обнуляем камеру
                API.triggerClientEvent(player, "AUTOBUY_SUCCESS_BUY");
                API.setEntityData(player, "AUTOBUY_IS_BUYING_CAR", false); // дата на то, что человек больше не в автосалоне    
                // координаты спавна рядом с автосалоном
                Vector3 defaultCarParkCoords = getDefaultParkCoords(carClass); // получаем координаты где будет дефолтно спавнится машина (если не введена команда /park ни разу)
                Vector3 defaultCarParkRotation = getDefaultParkRotation(carClass);// получаем rotation когда будет дефолтно спавнится машина (если не введена команда /park ни разу)
                double spawnX = defaultCarParkCoords.X;
                double spawnY = defaultCarParkCoords.Y;
                double spawnZ = defaultCarParkCoords.Z;
                double rot = defaultCarParkRotation.Z;
                // выдаем информацию купленной машине.
                autoInfo aInfo = new autoInfo(player, boughtVehicle.vHash, color1, color2, spawnX, spawnY, spawnZ, rot, boughtVehicle);
                API.setEntityData(boughtVehicle.Veh, "autoInfo", aInfo);
                allAutoInfo.Add(aInfo);
                vehicles.Add(boughtVehicle);
                // Добавляем в таблицу купленную машину. // поставив место спавна рядом с автосалоном 
                lifeRP_GM.mainClass.sqlCon.retSQLData(string.Format("INSERT INTO `autobuy` (`playerName`,`autoName`,`color1`,`color2`,`spawnX`,`spawnY`,`spawnZ`,`rotation`) VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')", player.name, boughtVehicle.Veh.model, color1, color2,spawnX,spawnY,spawnZ,rot));
                int carDimension = (int)API.getEntityData(player, "AUTOBUY_CURRENT_PLAYER_CAR_DIMENSION"); // получили дименшен где машины спавнились
                autoBuyDimensions.Remove(autoBuyDimensions.Find(x => x == carDimension)); // удалили его из листа дименшененов (нужно для нахождения свободного)
                // удаляем временную машину которая в автосалоне была
                tempVehicle.DeleteVehicle();
                // списываем с него деньги
                //pInfo.money = pInfo.money - carInfo.price; --------------------------------
                //pInfo.UpdateBD();
                //
                API.sendChatMessageToPlayer(player, "~y~[Server] ~g~Вы успешно приобрели машину.");
                API.sendChatMessageToPlayer(player, "~y~[Server] ~g~По умолчанию - место спавна рядом с автосалоном.");
                API.sendChatMessageToPlayer(player, "~y~[Server] ~r~Обязательно ~g~назначьте место спавна командой - /park.");
            }
        }
        public void parkCar(Client player)
        {
            if(player.isInVehicle)
            {
                try
                {
                    if (API.hasEntityData(player.vehicle, "autoInfo"))
                    {
                        autoInfo aInfo = (autoInfo)API.getEntityData(player.vehicle, "autoInfo");
                        if (aInfo.owner.name == player.name) // если владельцом является тот, кто прописал команду
                        {
                            Vector3 pos = API.getEntityPosition(player.vehicle);
                            Vector3 rot = API.getEntityRotation(player.vehicle);
                            double spawnX = pos.X; // новые координаты
                            double spawnY = pos.Y;
                            double spawnZ = pos.Z;
                            double rotation = rot.Z;
                            lifeRP_GM.mainClass.sqlCon.retSQLData(string.Format("UPDATE autobuy SET spawnX = '{0}', spawnY = '{1}' , spawnZ = '{2}', rotation = '{3}' WHERE spawnX = '{4}' AND spawnY = '{5}' AND spawnZ = '{6}' AND playerName = '{7}' AND autoName = '{8}' AND rotation = '{9}'", spawnX, spawnY, spawnZ, rotation, aInfo.spawnX, aInfo.spawnY, aInfo.spawnZ, player.name, aInfo.ownersVehicle.Veh.model, aInfo.rotation));
                            aInfo.spawnX = spawnX; // заменяем старые на новые
                            aInfo.spawnY = spawnY;
                            aInfo.spawnZ = spawnZ;
                            aInfo.rotation = rotation;
                            API.sendNotificationToPlayer(player, "~y~[Server] ~o~Новое место парковки - назначено.");
                        }
                        else
                            API.sendChatMessageToPlayer(player, "~y~[Server] ~r~Это не ваша машина.");
                    }
                }
                catch (Exception ex)
                {
                    ServerPlayers.playersClass.sendChatMessageToDevelopers("~r~[DevChat] ошибка в parkcar" + ex.Message + " Имя: " + player.name);
                    API.sendChatMessageToPlayer(player, "~r~Произошла ошибка при попытке назначения новой парковки.");
                }
            }   
        }
        public void testDrive(Client player)
        {
            if (API.hasEntityData(player, "AUTOBUY_IS_BUYING_CAR") && API.getEntityData(player, "AUTOBUY_IS_BUYING_CAR") == true)
            {
                string carClass = API.getEntityData(player, "AUTOBUY_CLASS");
                VehInfo tempVehicle = API.getEntityData(player, "AUTOBUY_CREATED_CAR"); // получаем выбранную машину
                Vector3 position = getRespawnCoords(carClass);
                Vector3 rotation = getRespawnRotation(carClass);
                int color1 = (int)API.getEntityData(player, "AUTOBUY_CAR_COLOR_ONE"); // получаем color1
                int color2 = (int)API.getEntityData(player, "AUTOBUY_CAR_COLOR_TWO"); // получаем color2
                VehInfo testDriveVehicle = new VehInfo(tempVehicle.vHash, position, rotation, color1, color2, 100); // создаем машину
                API.setPlayerIntoVehicle(player, testDriveVehicle.Veh, -1); // сажаем на вод.место     
                int carDimension = (int)API.getEntityData(player, "AUTOBUY_CURRENT_PLAYER_CAR_DIMENSION"); // получили дименшен где машины спавнились
                API.setEntityDimension(player, carDimension);
                API.sendChatMessageToPlayer(player, carDimension.ToString());
                API.freezePlayer(player, false); // фриз на false
                API.triggerClientEvent(player, "SET_CAMERA_FOR_CAR_BUY_TO_NULL"); // обнуляем камеру
                API.triggerClientEvent(player, "AUTOBUY_START_TEST_DRIVE");
                API.setEntityData(player, "AUTOBUY_IS_BUYING_CAR", false); // дата на то, что человек больше не в автосалоне    
                API.setEntityData(player, "AUTOBUY_IS_TESTING_CAR", true);
                API.setEntityData(player, "AUTOBUY_TEST_DRIVE_CAR", testDriveVehicle);
                tempVehicle.DeleteVehicle();
                API.sendChatMessageToPlayer(player, "~y~[Server] ~g~Вы находитесь в режмие тест-драйва.");
                API.sendChatMessageToPlayer(player, "~y~[Server] ~g~У вас есть 1 минута чтоб проверить машину.");
                API.sendChatMessageToPlayer(player, "~y~[Server] ~o~Чтобы прервать тест-драйв введите: /tdstop.");

                // timer
                testDriveTimer = new System.Timers.Timer(60000); //TODO: проверить
                testDriveTimer.AutoReset = true;
                testDriveTimer.Enabled = true;
                testDriveTimer.Elapsed += (sender, e) => OnTimedEventTestDriveTimer(sender, e, player);
                testDriveTimer.Start();
            }
        }
        public void stopTestDrive(Client player)
        {
            if (API.hasEntityData(player, "AUTOBUY_IS_TESTING_CAR") && API.getEntityData(player, "AUTOBUY_IS_TESTING_CAR") == true)
            {
                try
                {
                    string autoClass = API.getEntityData(player, "AUTOBUY_CLASS"); // получили в каком из автослаонов он
                    Vector3 position = getExitCoords(autoClass); // по автосалону получили координаты выхода.
                    API.setEntityData(player, "AUTOBUY_IS_BUYING_CAR", false);
                    API.setEntityData(player, "AUTOBUY_IS_TESTING_CAR", false);
                    API.setEntityPosition(player, position);
                    API.setEntityDimension(player, 0);
                    API.freezePlayer(player, false);
                    int carDimension = (int)API.getEntityData(player, "AUTOBUY_CURRENT_PLAYER_CAR_DIMENSION");
                    autoBuyDimensions.Remove(autoBuyDimensions.Find(x => x == carDimension)); // удалили из листа дименшененов (нужно для нахождения свободного)
                    VehInfo vehicle = API.getEntityData(player, "AUTOBUY_TEST_DRIVE_CAR"); // удаляем временную машину созданную во время тест драйва
                    vehicle.DeleteVehicle();
                    API.sendChatMessageToPlayer(player, "~y~[Server] ~r~Тест-драйв был закончен.");
                    
                }
                catch (Exception ex)
                {
                    ServerPlayers.playersClass.sendChatMessageToDevelopers("~r~[DevChat] ошибка в тест драйв" + ex.Message + " Имя: " + player.name);
                }
            }
        }

        private void OnTimedEventTestDriveTimer(System.Object source, ElapsedEventArgs e, Client player)
        {
            stopTestDrive(player);
            testDriveTimer.Stop();
        }

        public void sellCar(Client player)
        {
            if (player.isInVehicle)
            {
                try
                {
                    if (API.hasEntityData(player.vehicle, "autoInfo"))
                    {
                        autoInfo aInfo = (autoInfo)API.getEntityData(player.vehicle, "autoInfo");
                        if (aInfo.owner.name == player.name) // если владельцом является тот, кто прописал команду
                        {
                            lifeRP_GM.mainClass.sqlCon.retSQLData(string.Format("DELETE FROM autobuy WHERE spawnX = '{0}' AND spawnY = '{1}' AND spawnZ = '{2}' AND playerName = '{3}' AND autoName = '{4}' AND rotation = '{5}'", aInfo.spawnX, aInfo.spawnY, aInfo.spawnZ, player.name, aInfo.ownersVehicle.Veh.model, aInfo.rotation));
                            int carPrice = getCarPrice(aInfo.ownersVehicle.vHash);
                            PlayerInfo pInfo = API.getEntityData(player.handle, Constants.PlayerAccount);
                            pInfo.money = (int)(pInfo.money + (carPrice * 0.9));
                            player.vehicle.delete();
                            allAutoInfo.Remove(aInfo);
                            aInfo = null;
                            API.sendChatMessageToPlayer(player, string.Format("~y~[Server] ~o~Вы продали свою машину. Вы получили: ~g~{0}.",carPrice));
                        }
                    }
                }
                catch (Exception ex)
                {
                    ServerPlayers.playersClass.sendChatMessageToDevelopers("~r~[DevChat] ошибка в sellCar" + ex.Message + " Имя: " + player.name);
                    API.sendChatMessageToPlayer(player, "~r~Произошла ошибка при попытке продажи машины.");
                }
            }
        }
        public int getCarPrice(VehicleHash vHash)
        {
            sellCarInfo carInfo = null;
            // для каждого класса добавлять
            if (carInfo == null)
            {
                carInfo = autoHashes_Lowriders.Find(x => x.vHash == vHash);
            }
            if (carInfo == null)
            {
                carInfo = autoHashes_Muscle.Find(x => x.vHash == vHash);
            }
            if (carInfo == null)
            {
                carInfo = autoHashes_OffRoad.Find(x => x.vHash == vHash);
            }
            return carInfo.price;
        }
        public void setClassDatas(Client player, string autoClass) // используем в buyCar // назначаем класс автосалона где он находится
        {
            switch (autoClass)
            {
                case "Muscle":
                    API.setEntityData(player, "AUTOBUY_CLASS", "Muscle");
                    break;
                case "OffRoad":
                    API.setEntityData(player, "AUTOBUY_CLASS", "OffRoad");
                    break;
                case "Lowriders":
                    API.setEntityData(player, "AUTOBUY_CLASS", "Lowriders");
                    break;
            }
        }
        public Vector3 getExitCoords(string autoClass) // используем в exitBuy // получаем координаты выхода (во время отмены покупки)
        {
            Vector3 position = null;
            switch (autoClass)
            {
                case "Muscle":
                    position = new Vector3(-69.1979, 63.4519, 70.89);
                    break;
                case "OffRoad":
                    position = new Vector3(-202.24, -1157.81, 23.4945);
                    break;
                case "Lowriders":
                    position = new Vector3(-232.46, -1311.3, 31.296);
                    break;
            }
            return position;
        }
        public Vector3 getRespawnCoords(string autoClass) // получаем координаты где машина должна появится после покупки
        {
            Vector3 position = null; // 
            switch (autoClass)
            {
                case "Muscle":
                    position = new Vector3(-67.6892, 82.2818, 70.9253);
                    break;
                case "OffRoad":
                    position = new Vector3(-224.0033, -1162.331, 22.445);
                    break;
                case "Lowriders":
                    position = new Vector3(-205.92,-1306.57, 30.83);
                    break;
            }
            return position;
        }
        public Vector3 getRespawnRotation(string autoClass) // получаем rotation при повялении машины после покупки
        {
            Vector3 rotation = null;
            switch (autoClass)
            {
                case "Muscle":
                    rotation = new Vector3(0, 0, 64.764);
                    break;
                case "OffRoad":
                    rotation = new Vector3(0, 0, 90.2177);
                    break;
                case "Lowriders":
                    rotation = new Vector3(0, 0, -1.519478);
                    break;
            }
            return rotation;
        }
        public int getMaxCarIndex(string autoClass) // функция для получения максимального кол-ва машин в определенном автосалоне
        {
            int maxIndex = 0;
            switch (autoClass)
            {
                case "Muscle":
                    maxIndex = autoHashes_Muscle.Count; // максимальное кол-во машин
                    break;
                case "OffRoad":
                    maxIndex = autoHashes_OffRoad.Count; // максимальное кол-во машин
                    break;
                case "Lowriders":
                    maxIndex = autoHashes_Lowriders.Count; // максимальное кол-во машин
                    break;
            }
            return maxIndex;
        }
        public Vector3 getDefaultParkCoords(string autoClass) // координаты где машина будет появлятся если не ввдена команда /park
        {
            Vector3 position = null;
            switch (autoClass)
            {
                case "Muscle":
                    position = new Vector3(-85.59, 91.92, 71.77);
                    break;
                case "OffRoad":
                    position = new Vector3(-242.36, -1183.823, 22.45905);
                    break;
                case "Lowriders":
                    position = new Vector3(-191.303, -1290, 30.80);
                    break;
            }
            return position;
        }
        public Vector3 getDefaultParkRotation(string autoClass) // rotation где машина будет появлятся если не ввдена команда /park
        {
            Vector3 rotation = null;
            switch (autoClass)
            {
                case "Muscle":
                    rotation = new Vector3(0, 0, 154.70);
                    break;
                case "OffRoad":
                    rotation = new Vector3(0, 0, 1.001); 
                    break;
                case "Lowriders":
                    rotation = new Vector3(0, 0, -89.38);
                    break;
            }
            return rotation;
        }
        public Vector3 getAutoBuyCameraView(string autoClass)
        {
            Vector3 rotation = null;
            switch (autoClass)
            {
                case "Muscle":
                    rotation = new Vector3(1012.43, -3156.69, -37.907);
                    break;
                case "OffRoad":
                    rotation = new Vector3(1102.005, -3151.38, -37.51);
                    break;
                case "Lowriders":
                    rotation = new Vector3(-221.71, -1333.34, 30.89);
                    break;
            }
            return rotation;
        }
        public Vector3 getAutoBuyCameraRotation(string autoClass)
        {
            Vector3 rotation = null;
            switch (autoClass)
            {
                case "Muscle":
                    rotation = new Vector3(0, 0, 115.87);
                    break;
                case "OffRoad":
                    rotation = new Vector3(0, 0, 10.9578);
                    break;
                case "Lowriders":
                    rotation = new Vector3(0, 0, 21.244);
                    break;
            }
            return rotation;
        }
        public Vector3 getAutoBuyCarPosition(string autoClass)
        {
            Vector3 rotation = null;
            switch (autoClass)
            {
                case "Muscle":
                    rotation = new Vector3(1005.103, -3160.248, -39.477);
                    break;
                case "OffRoad":
                    rotation = new Vector3(1101.61, -3146.104, -38.088);
                    break;
                case "Lowriders":
                    rotation = new Vector3(-223.86, -1328.812, 30.39);
                    break;
            }
            return rotation;
        }
        public Vector3 getAutoBuyCarRotation(string autoClass)
        {
            Vector3 rotation = null;
            switch (autoClass)
            {
                case "Muscle":
                    rotation = new Vector3(0, 0, -120.2231);
                    break;
                case "OffRoad":
                    rotation = new Vector3(0, 0, 146.3377);
                    break;
                case "Lowriders":
                    rotation = new Vector3(0, 0, 164.79);
                    break;
            }
            return rotation;
        }
        public void createAutoBuyMarkersForEachClass() // создаем маркеры по классам (onResourceStart)
        {
            API.sleep(1000);
            // Class Muscle
            Vector3 positionMuscle = new Vector3(-69.1979, 63.4519, 70.89); // координаты маркера
            MarkerInfo autoBuyMarkerMuscle = new MarkerInfo(1, positionMuscle, new Vector3(1, 1, 1), new Vector3(35.5, 1, 1), new Vector3(1.5, 1.5, 2), 100, 23, 182, 71, 1, 2, Jobs.NULL, 0);
            autoBuyMarkerMuscle.SetData("autoBuyMarker", true);
            autoBuyMarkerMuscle.SetData("autoBuyMarker_CLASS", "Muscle");
            autoBuyMarkerMuscle.SetData("autoBuyMarkerPosition", positionMuscle);
            autoBuyEnterMarkers.Add(autoBuyMarkerMuscle);
            // Class OffRoad
            Vector3 positionOffRoad = new Vector3(-202.24, -1157.81, 22.4945); // координаты маркера
            MarkerInfo autoBuyMarkerOffRoad = new MarkerInfo(1, positionOffRoad, new Vector3(1, 1, 1), new Vector3(35.5, 1, 1), new Vector3(1.5, 1.5, 2), 100, 23, 182, 71, 1, 2, Jobs.NULL, 0);
            autoBuyMarkerOffRoad.SetData("autoBuyMarker", true);
            autoBuyMarkerOffRoad.SetData("autoBuyMarker_CLASS", "OffRoad");
            autoBuyMarkerOffRoad.SetData("autoBuyMarkerPosition", positionOffRoad);
            autoBuyEnterMarkers.Add(autoBuyMarkerOffRoad);
            // Class Lowriders
            Vector3 positionLowriders = new Vector3(-232.46, -1311.3, 30.296); // координаты маркера
            MarkerInfo autoBuyMarkerLowriders = new MarkerInfo(1, positionLowriders, new Vector3(1, 1, 1), new Vector3(35.5, 1, 1), new Vector3(1.5, 1.5, 2), 100, 23, 182, 71, 1, 2, Jobs.NULL, 0);
            autoBuyMarkerLowriders.SetData("autoBuyMarker", true);
            autoBuyMarkerLowriders.SetData("autoBuyMarker_CLASS", "Lowriders");
            autoBuyMarkerLowriders.SetData("autoBuyMarkerPosition", positionLowriders);
            autoBuyEnterMarkers.Add(autoBuyMarkerLowriders);
            // Class another
        }
        [Command("goo")]
        public void TPP(Client player)
        {
            Vector3 poss = new Vector3(-69.1979, 64.4519, 71.89); 
            API.setEntityPosition(player, poss);
        }
        [Command("goo1")]
        public void TP(Client player)
        {
            Vector3 poss = new Vector3(-202.24, -1157.81, 23.4945);
            API.setEntityPosition(player, poss);
        }
        [Command("goo2")]
        public void TPPP(Client player)
        {
            Vector3 poss = new Vector3(-232.46, -1311.3, 31.296);
            API.setEntityPosition(player, poss);
        }
        [Command("park")]
        public void CMD_parkCar(Client player)
        {
            parkCar(player);
        }
        [Command("test")]
        public void CMD_testDrive(Client player)
        {
            testDrive(player);
        }
        [Command("tdstop")]
        public void CMD_tdStop(Client player)
        {
            stopTestDrive(player);
        }
        //
        [Command("ss")]
        public void CMD_car(Client player)
        {
            int  d =API.getEntityDimension(player);
            API.sendChatMessageToPlayer(player, d.ToString());
        }
        [Command("pos")]
        public void pos(Client player)
        {
            Vector3 pos = API.getEntityPosition(player);
            Vector3 rot = API.getEntityRotation(player);
            API.sendChatMessageToPlayer(player,"x: " + pos.X + " y: " + pos.Y + " z: " + pos.Z + " rot: " + rot.Z);
        }
        [Command("dim")]
        public void dim(Client player)
        {
            int dimension = API.getEntityDimension(player);
            API.sendChatMessageToPlayer(player, dimension.ToString());
        }
        [Command("set")]
        public void set(Client player, int dim)
        {
            API.setEntityDimension(player, dim);
        }
        [Command("tp")]
        public void tp(Client player, double x, double y, double z)
        {
            API.setEntityPosition(player, new Vector3(x,y,z));
        }
        [Command("sellcar")]
        public void sellcar_CMD(Client player)
        {
            sellCar(player);
        }
    }
}