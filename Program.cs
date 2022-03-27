using System;
using System.Collections.Generic;
using System.Threading;
using SnakeEditors;

namespace AnySnakes
{
    /// <summary>Логика расстановки "яблок"</summary>
    class Apples
    {
        /// <summary>Считаем количество яблок в мире</summary>
        /// <param name="world">Двумерный массив мира</param>
        public static int Calculate(ref sbyte[,] world)
        {
            int c=0;
            for (int y=0; y<world.GetLength(1);y++)
                for (int x=0; x<world.GetLength(0);x++)
                    if (world[x,y]==1) c++;
            return c;
        }
        /// <summary>Добавляем нужное количество яблок в мир</summary>
        /// <param name="world">Двумерный массив мира</param>
        /// <param name="count">Число яблок</param>
        public static void Add(ref sbyte[,] world,int count=1)
        {  
            Random rnd = new Random();
            int x,y,i=0;
            while (i<count)
            {
                x=rnd.Next(0,world.GetLength(0));y=rnd.Next(0,world.GetLength(1));
                if (world[x,y]==0) {world[x,y]=1; i++;}
            }  
        }
        /// <summary>Создать яблоко</summary>
        /// <param name="world">Двумерный массив мира</param>
        /// <param name="maximum">Максимальное количество яблок</param>
        public static void Create(ref sbyte[,] world,int maximum)
        {   
            sbyte[] variants={1,1};
            int counter=0, c=(int)(world.GetLength(1)*world.GetLength(0)/maximum);
            for (int y=0;y<world.GetLength(1);y++)
                for (int x=0;x<world.GetLength(0);x++)
                {
                    counter++;
                    if (counter%c==0) world[x,y]=variants[new Random().Next(0,variants.Length)];
                }
        }
    }
    /// <summary>Класс описывает змейку.</summary>
    [Serializable]
    class Snake
    {
        ///<summary>Уникальный идентификатор змейки, где отображаются полученне мутации.</summary>
        public String Name = string.Empty;
        ///<summary>Массив весов взаимодействия с препятствиями рядом, размерности [11,11,4] (Матрица поведения)</summary>
        public sbyte[,,] MatrixOfFear    = new sbyte[11,11,4];  //484 байт
        ///<summary>Целочисленная Матрица весов взаимодействия с яблоками рядом, размерности [11,11,4] (Матрица поведения)</summary>
        public sbyte[,,] MatrixOfWish    = new sbyte[11,11,4]; //484 байт
        ///<summary>Очередь - координаты тела змейки в виде кортежа int x, int y</summary>
        public Queue<(int x,int y)> Body = new Queue<(int,int)>();// <---Очередь - мой выбор! ОЧЕРЕДЬ-ЭТО ТО, ЧТО НАМ НАДО! Используем кортеж из пары x,y. PS Мне где-то тут начинает нравиться шарп. https://docs.microsoft.com/ru-ru/dotnet/csharp/language-reference/builtin-types/value-tuples#code-try-4
        ///<summary>"Обдуманный" ход, характеризует направление движения следующего хода.</summary>
        private (sbyte x,sbyte y) ThinkedMove = (0,0);
        ///<summary> Состояние активности змейки.</summary>
        public bool isAlive=true;
        ///<summary>Возраст змейки в ходах</summary>
        public long Age=0;
        ///<summary>Дата "смерти" змейки</summary>
        public long DateOfDead = 0;
        ///<summary> Количество "потомков"</summary>
        public long NumberOfDescendants = 0;
        ///<summary>Отметка для визуализации выборок</summary>
        public bool Marked=false;
        ///<summary>Цвет змейки. Отражает мутации и прочий "богатый" внутренний мир.</summary>
        public sbyte Appearance = 11; 
        ///<summary> Словарь с цветами, 11-17 основные, 21-27 тёмные соответствующие для выделения</summary>
        public static Dictionary<int,ConsoleColor> MainColors = new Dictionary<int, ConsoleColor>()
        {   
            {11 ,ConsoleColor.Gray},
            {12 ,ConsoleColor.Cyan},
            {13 ,ConsoleColor.Blue},
            {14 ,ConsoleColor.Green},
            {15 ,ConsoleColor.Magenta},
            {16 ,ConsoleColor.Red},
            {17 ,ConsoleColor.Yellow},

            {21 ,ConsoleColor.Black},
            {22 ,ConsoleColor.Black},
            {23 ,ConsoleColor.Black},
            {24 ,ConsoleColor.Black},
            {25 ,ConsoleColor.Black},
            {26 ,ConsoleColor.Black},
            {27 ,ConsoleColor.Black}
        };
        /// <summary>Инициализация змейки</summary>
        /// <param name="world">Целочисленные координаты {x,y}</param>
        /// <returns></returns>
        public void Init(ref sbyte[,] world)
        {   
            // Если длина очереди Body == 0, то это первая змейка без "предка" и очередь с её телом не определена. сгенерируем тело длиною 5, от головы.
            if (Body.Count == 0) 
            {  
                int y=(int)(Console.WindowHeight/2);            // Пускай голова нулевой змейки будет по-центру консоли
                for (int x=(int)(Console.WindowWidth/2);x>(int)(Console.WindowWidth/2)-5;x--) 
                {
                    Body.Enqueue((x,y));
                    world[x,y]=Appearance;                      //Указать цвет
                }
            }
            return;
        }
        /// <summary>Рождение новой Змейки от текущего предка, мутация при рождении</summary>
        /// <param name="world">Целочисленные координаты {x,y}</param>
        /// <returns>Возвращает объект "Змейка"</returns>
        public Snake Born(ref sbyte[,] world)
        {   Random RND = new Random();
            if (Body.Count>12)
            {
                Snake NewBorned = new Snake();
                if (Marked) Appearance = (Appearance>20) ? Appearance-=10 : Appearance;  /// Сохраним цвет змейки для "потомка", чтобы выделение не влияло на цвет "потомка".
                NewBorned.Appearance=Appearance;
                NewBorned.Name = Name;
                CopyMatrix(ref MatrixOfFear,ref NewBorned.MatrixOfFear);                 /// Передаём содержимое матриц от предка потомку            
                CopyMatrix(ref MatrixOfWish,ref NewBorned.MatrixOfWish);
                if (RND.Next(0,5)==2)                                                    /// Вот тут и начинаются мутации! Вероятность 20%
                {
                    int I = RND.Next(0,NewBorned.MatrixOfWish.GetLength(0));
                    int J = RND.Next(0,NewBorned.MatrixOfWish.GetLength(1));
                    int K = RND.Next(0,NewBorned.MatrixOfWish.GetLength(2));
                    sbyte C = (sbyte)RND.Next(-10,11);

                    /// Выбираем матрицу для изменения и следим, чтобы значение оставалось в диапазоне от -99 до 99
                    if (Marked) Appearance = (Appearance>20) ? Appearance-=10 : Appearance; 
                    if (RND.Next(0,2)==1) 
                    {
                        NewBorned.MatrixOfWish[I,J,K]+=C;
                        NewBorned.MatrixOfWish[I,J,K] = 
                            NewBorned.MatrixOfWish[I,J,K] <-99 ? NewBorned.MatrixOfWish[I,J,K]=-95 : 
                            NewBorned.MatrixOfWish[I,J,K] > 99 ? NewBorned.MatrixOfWish[I,J,K]= 95 : NewBorned.MatrixOfWish[I,J,K];
                        NewBorned.Name = NewBorned.Name + "W";
                    }
                    else
                    {
                        NewBorned.MatrixOfFear[I,J,K]+=C;
                        NewBorned.MatrixOfFear[I,J,K] = 
                            NewBorned.MatrixOfFear[I,J,K] <-99 ? NewBorned.MatrixOfFear[I,J,K]=-95 : 
                            NewBorned.MatrixOfFear[I,J,K] > 99 ? NewBorned.MatrixOfFear[I,J,K]= 95 : NewBorned.MatrixOfFear[I,J,K];
                         NewBorned.Name = NewBorned.Name + "F";
                    }
                    NewBorned.Appearance++; if (NewBorned.Appearance>17) NewBorned.Appearance=11; // Назначаем новый цвет мутировавшему "потомку".
                }
                Queue<(int x,int y)> BodyOfOldSnake = new Queue<(int,int)>();                     // А теперь тело режем напополам. Начинаем давить в две новые очереди.
                while (Body.Count>(12/2)) BodyOfOldSnake.Enqueue(Body.Dequeue());                 // Первая половина старой Змейки.
                Stack<(int x,int y)> BodyOfNewSnake = new Stack<(int,int)>();
                while (Body.Count>0) BodyOfNewSnake.Push(Body.Dequeue());                         // Вторая половина старой Змейки в стек. Потому что нужно развернуть новорождённую змейку
                while (BodyOfOldSnake.Count>0) Body.Enqueue(BodyOfOldSnake.Dequeue());            // Возвращаем очередь с телом назад.
                while (BodyOfNewSnake.Count>0) NewBorned.Body.Enqueue(BodyOfNewSnake.Pop());      // Записываем новое тело новорождённой. 
                NumberOfDescendants++;                                          //Мы хотим знать - сколько потомков оставила змейка. Это нужно для последующего анализа.
                return NewBorned;
            }
            return null;
        }
        /// <summary>Копирование многомерного массива из А(источник) в B(целевой)</summary>
        public void CopyMatrix (ref sbyte[,,] A, ref sbyte[,,] B)
        {
            for (int i=0; i<A.GetLength(0);i++)
                for (int j=0; j<A.GetLength(1);j++)
                    for (int k=0; k<A.GetLength(2);k++)
                        B[i,j,k]=A[i,j,k];
        }        
        /// <summary>Выполняем расчёт направления движения по матрицам.</summary>
        /// <param name="world">Двумерный массив мира</param>
        public void ThinkMove (ref sbyte[,] world)
        {
            if (isAlive)
            {
                (int x,int y) Head = Body.Peek();
                ///Подготовим участок массива "раздражителей" размерностью, как в матрицах. Пусть он будет трёхмерный, нулевой слой - слой с яблоками, первый слой - слой с препятствиями
                ///Более понятное временное решение. Можно сильно упростить вообще избавившись от этого массива. Пока сделал так.
                sbyte[,,] SnakeEyes = new sbyte[11,11,2];
                int i,j;
                for (int y=-5;y<=5;y++)
                {   j=Head.y+y;
                    if (j>world.GetLength(1)-1) j-=(world.GetLength(1)-1);
                    if (j<0) j+=(world.GetLength(1)-1);
                    for (int x=-5;x<=5;x++)
                    {
                        i=Head.x+x;
                        if (i>world.GetLength(0)-1) i-=(world.GetLength(0)-1);
                        if (i<0) i+=(world.GetLength(0)-1);
                        if (world[i,j]==1) SnakeEyes[x+5,y+5,0]=1;
                        if (world[i,j]> 1) SnakeEyes[x+5,y+5,1]=1;
                    }
                }
                int[] wish = new int[4];                                  // как матрицах [Лево,Низ,Верх,Право]
                for (int y=0;y<11;y++)
                    for (int x=0;x<11;x++)
                        for (int z=0;z<4;z++)
                            wish[z]+=(int)(SnakeEyes[y,x,0])*MatrixOfWish[x,y,z]+(int)(SnakeEyes[y,x,1])*MatrixOfFear[x,y,z];
                
                List<int> k= new List<int>(){0};                          // Индекс, определяющий напраление.
                for (int l=0; l<4;l++) if (wish[k[0]]<=wish[l]) k[0]=l;   // Получаем индекс самого большого элемента.
                for (int l=0; l<4;l++) if (wish[k[0]]==wish[l]) k.Add(l); // Соберём все равные большему индексы
                k.Remove(0);                                              // Нулевой задвоится. Уберём из списка.
                switch (k[new Random().Next(0,k.Count)])                  // Выбираем направление (случайно) из списка равных большему элементов 
                {
                    case (0): ThinkedMove=(-1,0);break;
                    case (1): ThinkedMove= (0,1);break;
                    case (2): ThinkedMove=(0,-1);break;
                    case (3): ThinkedMove= (1,0);break;
                }                                                         
            }
            else
            {
                ThinkedMove = (0,0);                                      // Если змейка мертва - она никуда не движется.
            }
            return;
        }
        /// <summary>Ход змейки. Змейка растёт, когда голова получает те же координаты, что и яблоко,
        //// каждый _десятый(?)_ ход будет приводить к "похудению" змейки на одну клетку.</summary>
        /// <param name="world">Двумерный массив мира</param>
        public void Move(ref sbyte[,] world)
        {      
                int c=1;                                    //Переменная указывает на поведение змейки при ходе: 1 - не меняется, 0 - удлиняется, 2 - сокращается.   
                if (Marked) Appearance = (Appearance<20) ? Appearance+=10 : Appearance -=10; //Выделенная змейка мигает каждый ход меняя свой цвет по словарю.
                (int x,int y) Head = Body.Peek();
                Head = ((int)(Head.x + ThinkedMove.x),(int)(Head.y+ThinkedMove.y));                                  //Двигаем голову согласно заранее принятому решению
                if (Head.x > world.GetLength(0)-1) Head.x = 0; if (Head.x < 0) Head.x = (int)(world.GetLength(0)-1); //Мир считаем замкнутым, гомеоморфным тору.
                if (Head.y > world.GetLength(1)-1) Head.y = 0; if (Head.y < 0) Head.y = (int)(world.GetLength(1)-1);
                if (Body.Count<3) {Die(ref world); return;}             // Змейка недоедала. Такая Змейка нам не нужна.
                if (world[Head.x,Head.y]>1) {Die(ref world);return;}    // Если голова совместилась с чемто кроме пустоты или яблока - змейка не живёт.
                if (world[Head.x,Head.y]==1) c=0;                       // Если яблоко совместилась с головой - удлиняемся
                world[Head.x,Head.y]=Appearance;                        // Помещаем голову в мир
                if (Age%15==0) c=2;                                     // Решаем - будет ли худеть змейка

                //Переписываем очередь тела змейки с новыми координатами. Первой полезет в очередь голова, далее вся очередь до предпоследнего элемента

                Queue<(int x,int y)> TemporaryBody = new Queue<(int,int)> {};TemporaryBody.Enqueue(Head);
                while (Body.Count>c) 
                {
                    if (Marked)
                        world[Body.Peek().x,Body.Peek().y]=Appearance;  //Перерисовываем нужным цветом, чтобы выделенная змейка подсвечивалась, если уж она выделена.
                    TemporaryBody.Enqueue(Body.Dequeue());              //Новое положение тела готово. Если яблоко съедено, хвост не затираем и очередь увеличится на 1, т.е. считываем до конца. 
                } 
                if (c>0 && Body.Count==c)                               //Не забыть затереть оставшийся хвост в мире world, если яблоко не съедено.
                    for (int k=0; k<c;k++) 
                    {
                        world[Body.Peek().x,Body.Peek().y]=0; 
                        Body.Dequeue();
                    }    
                Body.Clear(); // На всякий случай зачищаем Body, хотя в этот момент он уже должен быть пуст.
                Body=TemporaryBody;
            Age++;
            return;
        }
        /// <summary>Очистка мира от Змейки</summary>
        /// <param name="world">Двумерный массив мира</param>
        public void Die(ref sbyte[,] world)
        {   
            isAlive=false;
            while (Body.Count>0)
            {
                (int x,int y) Part =  Body.Dequeue();
                world[Part.x,Part.y]=0;
            }
        }
    }
    class Program
    {   /// <summary>Матрица мира(предыдущий кадр)</summary>
        static sbyte[,] OldWorld = new sbyte[Console.WindowWidth, Console.WindowHeight-1]; 
        /// <summary>Матрица мира, где каждый элемент содержит:
        /// 0-Свободное место;
        /// 1-Яблоко; 
        /// 8-Препятствие (для обучения змеек работе с препятствиями); 
        /// от 11 и более тело змейки. Используется для цветовой дифференциации поколений.</summary>
        static sbyte[,] NewWorld = new sbyte[Console.WindowWidth, Console.WindowHeight-1];
        public static void Main()
        { 
            Console.Clear();
            Console.WriteLine($"{Console.WindowHeight}, {Console.WindowWidth}");
            Console.WriteLine($"{Console.CursorTop}, {Console.CursorLeft}");
            ConsoleKeyInfo cki;
            ConsoleColor BaseForegroundColor=Console.ForegroundColor;
            bool play=true, Visualize=true;
            byte Temp=50; long AgeOfWorld=0,LongestLived=0,MostProlific=0;
            bool showHelp = false;
            int countOfApples=159,  countOfApples_now=0;  //Задаём количество яблок в мире
            int snakeSelector=0;

            List<Snake> Snakes = new List<Snake>(){new Snake()};        //Список всех змеек мира
            List<Snake> DiedSnakesCollector = new List<Snake>(){};      //Сборщик мёртвых змеек
            List<Snake> CanBornSnakesCollector = new List<Snake>(){};   //Сборщик змеек к размножению
            Queue<Snake> SnakesLongestLived = new Queue<Snake>(){};     //Очередь самых долгоживущих змеек для отбора.
            Queue<Snake> SnakesMostProlific = new Queue<Snake>(){};     //Очередь змеек, более успешно размножающихся.
            
            Console.ReadKey();
            
            
            
            Snake A=Snakes[0];                          // Змейка для выбора. Первый объект в мире.
            
            worldEditor.Setup(in A, out int countOfEpochs, out long periodOfSelection, out countOfApples);   // Запускаем меню изменяемых параметров нового мира.
            
            Apples.Create(ref NewWorld,countOfApples);
            SnankeEditor.Loop (ref A);                  // Сразу запускаем редактор для выбора параметров
            if (A.Body.Count<3) A.Init(ref NewWorld);   // Если была загружена змейка и у неё пустое тело, проинициализируем.
            Snakes[0]=A;

            Console.CursorVisible = false;
            Console.Clear();
            do {                                        // ГЛАВНЫЙ ЦИКЛ
                if (Snakes.Count==0) return;        
                if (play)                               // Если поставлено на паузу - можно вполнять выбор змейки и управление через редактор
                {    
                    countOfApples_now = Apples.Calculate(ref NewWorld); // Дополняем яблоки
                    if (countOfApples_now<countOfApples) Apples.Add(ref NewWorld,countOfApples-countOfApples_now);
                    foreach (Snake S in Snakes)
                    {
                        S.ThinkMove(ref NewWorld);      // Каждая змейка обдумывает ход
                        S.Move(ref NewWorld);           // Каждая змейка совершает ход
                        if (S.isAlive==false)           // Обработаем более не активных змеек
                        {
                            S.DateOfDead=AgeOfWorld;      /// Устанавливаем дату "смерти".
                            DiedSnakesCollector.Add(S);   /// Собираем "души" (объекты отмеченные как мёртвые с пустым телом) умерших Змеек
                            if (S.Age>LongestLived)       /// Если "Змейка" жила дольше всех предыдущих - запоминаем её в очередь  
                            {
                                LongestLived=S.Age;
                                SnakesLongestLived.Enqueue(S);
                                if (SnakesLongestLived.Count>20) SnakesLongestLived.Dequeue();
                                if (!Visualize) showTableOfLeaders (new Queue<Snake> (SnakesLongestLived), new Queue<Snake> (SnakesMostProlific));
                            }
                            if (S.NumberOfDescendants>MostProlific)       /// Если "Змейка" дала больше всех потомков - запоминаем её в очередь  
                            {
                                MostProlific=S.NumberOfDescendants;
                                SnakesMostProlific.Enqueue(S);
                                if (SnakesMostProlific.Count>20) SnakesMostProlific.Dequeue();
                                if (!Visualize) showTableOfLeaders (new Queue<Snake> (SnakesLongestLived), new Queue<Snake> (SnakesMostProlific));
                            }
                        }
                        if (S.Body.Count>12) CanBornSnakesCollector.Add(S);                                         // Создаём список змеек к размножению.
                    }
                    if (DiedSnakesCollector.Count>0)                                                                //Удаляем остатки неактивных змеек
                        {foreach (Snake S in DiedSnakesCollector) Snakes.Remove(S);DiedSnakesCollector.Clear();}    
                    if (CanBornSnakesCollector.Count>0)                                                             // Размножаем Змеек
                        {foreach (Snake S in CanBornSnakesCollector) Snakes.Add(S.Born(ref NewWorld));CanBornSnakesCollector.Clear();} 
                    AgeOfWorld++;
                    if (AgeOfWorld>periodOfSelection) 
                    {
                        AgeOfWorld=0;
                        ClearWorld(ref SnakesLongestLived,ref SnakesMostProlific,ref Snakes,ref NewWorld, countOfApples);
                        
                        LongestLived=0;  
                        MostProlific=0;
                        Console.Clear();
                    }
                }
                if (Visualize) //Если требуется визуализация отрисовываем новый кадр
                {
                    for (byte y=0;y<NewWorld.GetLength(1);y++)
                    {
                        for (byte x=0;x<NewWorld.GetLength(0);x++)
                        {
                            if (NewWorld[x,y]!=OldWorld[x,y])
                            {
                                Console.SetCursorPosition(x,y); 
                                switch (NewWorld[x,y])
                                {
                                    case 0  :  Console.Write(" "); break;
                                    case 1  :  Console.Write("●"); break;
                                    case 8  :  Console.Write("🞓"); break;
                                    case >10:  Console.ForegroundColor=(Snake.MainColors[NewWorld[x,y]]);  /// ЭТО НЕ РАБОТАЕТ ПРИ КОМПИЛЯЦИИ ПОД MONO. 
                                               Console.Write("█");
                                               Console.ForegroundColor=BaseForegroundColor;
                                               break;
                                }
                                OldWorld[x,y]=NewWorld[x,y];
                            }
                        }
                    }
                    Thread.Sleep(Temp);
                }
                /// Выводим "подвал"
                Console.SetCursorPosition(0,Console.WindowHeight-1);
                
                if (showHelp)
                {
                    Console.Write ($"F1-Помощь  Популяция:{Snakes.Count} | Возраст мира:{AgeOfWorld}");
                }
                else
                {
                    Console.Write ("F1-Информация; V-Визуализация ВКЛ/ВЫКЛ; Pause-Пауза; ← → - выбор змейки; F5-Редактор;");
                }


                Console.CursorVisible = false;
                if (Console.KeyAvailable) 
                {
                    cki = Console.ReadKey(true);
                    switch (cki.Key)
                    {
                        case (ConsoleKey.PageDown)  : Temp++; Temp = Temp >254 ? Temp-- : Temp;     break;
                        case (ConsoleKey.PageUp )   : Temp--; Temp = Temp <2   ? Temp++ : Temp;     break;     

                        case (ConsoleKey.Pause)  : play = play ? false : true;     break;    // Пауза
                        case (ConsoleKey.V)         : Visualize = Visualize ? false : true;     // Визуализация ВКЛ/ВЫКЛ
                                                      Console.Clear();  
                                                      showTableOfLeaders (new Queue<Snake> (SnakesLongestLived), new Queue<Snake> (SnakesMostProlific));
                                                      Console.Clear();
                                                      break;

                        case (ConsoleKey.LeftArrow) : if (snakeSelector>Snakes.Count-1)         // Выбор змейки
                                                            snakeSelector=Snakes.Count-1; 
                                                      A.Marked=false; 
                                                      snakeSelector--;  
                                                      break;

                        case (ConsoleKey.RightArrow): if (snakeSelector>Snakes.Count-1) 
                                                            snakeSelector=Snakes.Count-1; 
                                                      A.Marked=false; 
                                                      snakeSelector++;  
                                                      break;

                        case (ConsoleKey.F5)        : SnankeEditor.Loop(new Queue<Snake> (SnakesLongestLived), new Queue<Snake> (SnakesMostProlific));  break; //Редактор выбранной
                        case (ConsoleKey.F6)        : MatrixEditor.Loop(ref A.MatrixOfFear);  break;
                        case (ConsoleKey.F8)        : SnankeEditor.Loop(ref A);               break;
                        case (ConsoleKey.F1)        : showHelp = showHelp ? false : true;     break;
                    }
                            if (snakeSelector>Snakes.Count-1) snakeSelector=0;
                            if (snakeSelector<0) snakeSelector=Snakes.Count-1;
                            A = Snakes[snakeSelector];
                            Snakes[snakeSelector].Marked=true;
                }
             } while(true);
        }
        ///<summary>Очистка мира с сожранением лучших змеек. Искусственная часть отбора.</summary>
        private static void ClearWorld(ref Queue<Snake> SnakesLongestLived,ref Queue<Snake> SnakesMostProlific,ref List<Snake> Snakes,ref sbyte[,] World,int countOfApples)
        {   Snakes.Clear();
            Snake Staff;
                while (SnakesLongestLived.TryDequeue(out Staff)) if (!Snakes.Contains(Staff)) Snakes.Add(Staff); //добавляем так чтобы не повторялись
                while (SnakesMostProlific.TryDequeue(out Staff)) if (!Snakes.Contains(Staff)) Snakes.Add(Staff);

                for (int i=0;i<World.GetLength(0);i++)                                  //Очищаем мир
                    for (int j=0;j<World.GetLength(1);j++)
                        World[i,j]=0;
                Apples.Create(ref World,World.GetLength(0)*World.GetLength(1)-10);      //Заполняем яблоками

                int x=0, y=World.GetLength(1)/2;                                        //Делаем новые тела Змейкам, размещая их равномерно, чтобы поместились все
                (int x,int y) Part;
                foreach (Snake S in Snakes)
                {
                    for (int c=0;c<6;c++) 
                    {
                        Part.x=x;Part.y=y+c;
                        S.Body.Enqueue(Part);
                    }
                    x+=3;
                    S.isAlive=true;                                                     // Не забыть оживить!
                    S.Age=0;
                    S.NumberOfDescendants=0;
                }
            return;
        }
        ///<summary> Показать лидеров мира.</summary>
        private static void showTableOfLeaders (Queue<Snake> SnakesLongestLived, Queue<Snake> SnakesMostProlific)
        {
            Console.SetCursorPosition (0,3); 
            string label;
            Snake Staff;
            Console.WriteLine ("╔"+(new string ('═',(Console.WindowWidth-29)/2))+"Таблица лидеров по возрасту"+(new string ('═',(Console.WindowWidth-30)/2))+"╗");
            while (SnakesLongestLived.TryDequeue(out Staff))
            {
                label = $"Дата: {Staff.DateOfDead},Возраст: {Staff.Age}, потомков:{Staff.NumberOfDescendants}, Мутаций:{Staff.Name.Length}";
                Console.WriteLine (new string (' ',(Console.WindowWidth-label.Length)/2+1)+label);
            }
            Console.SetCursorPosition (0,24);
            Console.WriteLine       ("╚"+(new string ('═',(Console.WindowWidth-2)))+"╝");
            
            Console.SetCursorPosition (0,Console.WindowHeight-24);
            Console.WriteLine       ("╔"+(new string ('═',(Console.WindowWidth-33)/2))+"Таблица лидеров по плодовитости"+(new string ('═',(Console.WindowWidth-34)/2))+"╗");
            while (SnakesMostProlific.TryDequeue(out Staff))
            {
                label = $"Дата: {Staff.DateOfDead},Возраст: {Staff.Age}, потомков:{Staff.NumberOfDescendants}, Мутаций:{Staff.Name.Length}";
                Console.WriteLine (new string (' ',(Console.WindowWidth-label.Length)/2+1)+label);
            }
            Console.SetCursorPosition (0,Console.WindowHeight-2);
            Console.WriteLine       ("╚"+(new string ('═',(Console.WindowWidth-2)))+"╝");
            return;
        }
    }
}
