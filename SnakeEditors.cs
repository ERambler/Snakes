using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using AnySnakes;

namespace SnakeEditors
{
    ///<summary>Редактор объекта(ов) змейки.</summary>
    class SnankeEditor
    {
        ///<summary> Формирование списка из очередей</summary>
        /// <param name="SnakesLongestLived">Очередь отбора змеек самых долгоживущих</param>
        /// <param name="SnakesMostProlific">Очередь отбора змеек самых плодовитых</param>
        /// <returns></returns>
        private static List<Snake> initListEdit (Queue<Snake> SnakesLongestLived, Queue<Snake> SnakesMostProlific)
        {   
            List<Snake> Snakes = new List<Snake>();
            Snake Staff;
            while (SnakesLongestLived.TryDequeue(out Staff)) if (!Snakes.Contains(Staff)) Snakes.Add(Staff); //добавляем так чтобы не повторялись
            while (SnakesMostProlific.TryDequeue(out Staff)) if (!Snakes.Contains(Staff)) Snakes.Add(Staff);
            return Snakes;
        }
        ///<summary> Вывести на экран список отобранных змеек.</summary>
        private static void showTable (ref List<Snake> Snakes, int activeInput)
        {
            string label = "Таблица результатов отбора:";
            Console.SetCursorPosition((Console.WindowWidth / 2) - (label.Length / 2),(Console.WindowHeight / 2) - 20-2); Console.Write(label);
            Console.SetCursorPosition((Console.WindowWidth / 2)-25, (Console.WindowHeight / 2) - 20-1); Console.Write("  №  {0,10} {1,10} {2,10} {3,10} ","Дата","Возраст","Потомков","Мутаций");
            label = "↑ ↓ - Указать объект; ↲ - Выбрать объект";
            Console.SetCursorPosition((Console.WindowWidth / 2) - (label.Length / 2), Console.WindowHeight-4); Console.Write(label);

            int i=0; 
            ConsoleColor DefaultColor = Console.BackgroundColor;
            foreach (var item in Snakes)
            {
                Console.SetCursorPosition ((Console.WindowWidth/2)-25,(Console.WindowHeight / 2) - 20+i);
                if (activeInput==i) Console.BackgroundColor = ConsoleColor.DarkGray;
                Console.Write("{0,3}.{1,10} {2,10} {3,10} {4,10} ",i+1,item.DateOfDead,item.Age,item.NumberOfDescendants,item.Name.Length);
                Console.BackgroundColor = DefaultColor;
                i++;
            }
            return;            
        }
        ///<summary> Меню выбора результатов отбора.(Передавать копию! Очередь будет пуста!)</summary>
        public static void Loop(Queue<Snake> SnakesLongestLived, Queue<Snake> SnakesMostProlific)
        {
            
            bool loop = true;
            int activeInput = 0;
            List<Snake> Snakes = initListEdit(SnakesLongestLived,SnakesMostProlific);
            Snake Staff;
            ConsoleKeyInfo cki;
            Console.CursorVisible = false;
            Console.Clear();
            showTable(ref Snakes,activeInput);
            do 
            {   
                if (Console.KeyAvailable) 
                {
                    cki = Console.ReadKey(true);
                    switch (cki.Key)
                    {
                        case (ConsoleKey.DownArrow) : activeInput++; break;
                        case (ConsoleKey.UpArrow)   : activeInput--; break;
                        case (ConsoleKey.Escape)    : loop=false; break;
                        case (ConsoleKey.Enter)     : Staff = Snakes[activeInput]; Loop(ref Staff); showTable(ref Snakes,activeInput); break;
                    }
                    if (activeInput<0) activeInput=Snakes.Count-1; if (activeInput>Snakes.Count-1) activeInput=0;
                    showTable(ref Snakes,activeInput); //Обновляем выделение
                }
                Thread.Sleep(15);
            } while(loop);
            Console.Clear();
        }

        ///<summary> Вывести на экран список вартиантов изменения змейки.</summary>
        private static void showTable (ref Snake W,ref Dictionary<string,string> Sheet, int activeInput)
        {   
            Console.Clear();
            string label = "Редактор Змейки";
            Console.SetCursorPosition((Console.WindowWidth / 2) - (label.Length / 2), 1);Console.Write(label);
            label = "↑ ↓ - Указать элемент; ↲ - Изменить элемент";
            Console.SetCursorPosition((Console.WindowWidth / 2) - (label.Length / 2), Console.WindowHeight-4);Console.Write(label);
            label = "F10 - Загрузить из файла; F12 - Сохранить в файл; Esc - Выход (Изменения будут сохранены)";
            Console.SetCursorPosition((Console.WindowWidth / 2) - (label.Length / 2), Console.WindowHeight-2);Console.Write(label);

            int i=0; ConsoleColor DefaultColor = Console.ForegroundColor;
            foreach (var item in Sheet)
            {
                Console.SetCursorPosition (Console.WindowWidth/4,(2*i)+3);
                if (activeInput==i) Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{item.Key} {item.Value}");
                Console.ForegroundColor = DefaultColor;
                i++;
            }
        }
        private static void LoadCaptions (ref Snake W, out Dictionary<string,string> Sheet)
        {
            Dictionary<string,string> S =new Dictionary<string, string>()
            {
                {"Имя",W.Name},
                {"Активность",W.isAlive.ToString()},
                {"Возраст",W.Age.ToString()},
                {"Потомки",W.NumberOfDescendants.ToString()},
                {"Цвет",""},    //Заглушка. Позднее сделаем выбор цвета из предлагаемых в данной консоли (Совместимость и все дела)
                {"Матрица взаимодействия с препятствиями",String.Empty},
                {"Матрица взаимодействия с яблоками",String.Empty}
            };
            Sheet = S;
        }
        private static void Change (ref Dictionary<string,string> Sheet,string Key,ref Snake snake)
        {
            switch (Key)
            {
                case ("Активность"): if (Sheet[Key]=="False") Sheet[Key]="True"; else Sheet[Key]="False"; break;
                case ("Матрица взаимодействия с препятствиями"): MatrixEditor.Loop(ref snake.MatrixOfFear); break;
                case ("Матрица взаимодействия с яблоками"):      MatrixEditor.Loop(ref snake.MatrixOfWish); break;
            }
        }
        public static void Loop(ref Snake W)
        {
            bool loop = true;
            int activeInput = 0;
            Dictionary<string,string> Sheet;
            LoadCaptions (ref W,out Sheet);
            showTable (ref W,ref Sheet, activeInput);
            ConsoleKeyInfo cki;
            do 
            {   Console.CursorVisible = false;
                if (Console.KeyAvailable) 
                {
                    cki = Console.ReadKey(true);
                    switch (cki.Key)
                    {
                        case (ConsoleKey.DownArrow) : activeInput++; break;
                        case (ConsoleKey.UpArrow)   : activeInput--; break;
                        case (ConsoleKey.Enter)     : Change(ref Sheet,Sheet.ElementAt(activeInput).Key,ref W); break;
                        case (ConsoleKey.F12)       : Save(SelectFile("snake"), ref W); break;
                        case (ConsoleKey.F10)       : Load(SelectFile("snake",false),ref W); break;
                        case (ConsoleKey.Escape)    : loop=false; break;
                    }
                    if (activeInput<0) activeInput=Sheet.Count-1; if (activeInput>Sheet.Count-1) activeInput=0;
                    showTable (ref W,ref Sheet, activeInput);
                }
                    Thread.Sleep(15);
            } while(loop);
            Console.Clear();
        }
        /// <summary>Подбор имени файла из текущего каталога, ввод имени вручную</summary>
        /// <param name="FileExt">Расширение имени файла без точки.</param>
        /// <param name="write">Выбор файла для записи - true, для чтения false.</param>
        /// <returns>Строка с выбранным именем файла включая расширение.</returns>
        public static string SelectFile (string FileExt,bool write = true)
        {
            bool loop = true;
            int activeInput = 0;
            ConsoleKeyInfo cki;
            ConsoleColor DefaultColor = Console.BackgroundColor;
            Console.Clear();
            string title = "Выбор имени файла по расширению: ".ToUpper() + FileExt;
            Console.SetCursorPosition((Console.WindowWidth / 2) - (title.Length / 2), 1);Console.Write(title);
            title = "↑ ↓ - Выбрать существующее имя; ↲ - Указать имя файла; Esc - Отмена";
            Console.SetCursorPosition((Console.WindowWidth / 2) - (title.Length / 2), Console.WindowHeight-4);Console.Write(title);
            title = "Расширение файла входит в имя файла";
            Console.SetCursorPosition((Console.WindowWidth / 2) - (title.Length / 2), Console.WindowHeight-2);Console.Write(title);
            for (int e=4;e<Console.WindowHeight-6;e++) 
            {
                Console.SetCursorPosition((Console.WindowWidth/4)-1 ,e);
                Console.Write($"║{new string(' ',((int)(Console.WindowWidth/2)+1))}║");
            }
            Console.SetCursorPosition((Console.WindowWidth/4)-1 , 3); Console.Write($"╔{new string('═',(Console.WindowWidth/2)+1)}╗");
            Console.SetCursorPosition((Console.WindowWidth/4)-1 , Console.WindowHeight-6); Console.Write($"╚{new string('═',((int)(Console.WindowWidth/2)+1))}╝");
            Console.SetCursorPosition((Console.WindowWidth/4)-1 , Console.WindowHeight-8); Console.Write($"╠{new string('═',((int)(Console.WindowWidth/2)+1))}╣");

            string[] files = Directory.GetFiles(".", "*."+FileExt);

            for (int i=0; i<files.Length;i++)
                {
                    Console.SetCursorPosition (Console.WindowWidth/4,i+4);
                    if (activeInput==i) Console.BackgroundColor = ConsoleColor.DarkGray;
                    Console.Write(files[i]);
                    Console.BackgroundColor = DefaultColor;
                }
                Console.SetCursorPosition (Console.WindowWidth/4,(files.Length)+4);
                if (write) Console.Write("-=СОЗДАТЬ НОВЫЙ ФАЙЛ=-"); else Console.Write("-=ВВЕСТИ ВРУЧНУЮ=-");

            do 
            {   Console.CursorVisible = false;
                if (Console.KeyAvailable) 
                {
                    cki = Console.ReadKey(true);
                    switch (cki.Key)
                    {
                        case (ConsoleKey.DownArrow) :activeInput++; break;
                        case (ConsoleKey.UpArrow)   :activeInput--; break;
                        case (ConsoleKey.Enter)     :loop=false; break;
                        case (ConsoleKey.Escape)    :activeInput=-1;loop=false; break;
                    }
                    //if (activeInput<0) activeInput=files.Length; 
                    if (activeInput>files.Length) activeInput=0;

                    //Отрисовываем список имён файлов

                    for (int i=0; i<files.Length;i++)
                    {
                        Console.SetCursorPosition (Console.WindowWidth/4,i+4);
                        if (activeInput==i) Console.BackgroundColor = ConsoleColor.DarkGray;
                        Console.Write(files[i]);
                        Console.BackgroundColor = DefaultColor;
                    }

                    Console.SetCursorPosition (Console.WindowWidth/4,(files.Length)+4);
                    if (activeInput==files.Length) Console.BackgroundColor = ConsoleColor.DarkGray;
                    if (write) Console.Write("-=СОЗДАТЬ НОВЫЙ ФАЙЛ=-"); else Console.Write("-=ВВЕСТИ ВРУЧНУЮ=-");
                    Console.BackgroundColor = DefaultColor;
                }
                
                Thread.Sleep(15);
            } while(loop);

            string FileName;
            if (activeInput==-1) {return "";} 
            if (activeInput==files.Length)
            {
                Console.SetCursorPosition (Console.WindowWidth/4, Console.WindowHeight-7);
                Console.CursorVisible = true;FileName=Console.ReadLine();
            }
            else
            {
                FileName=files[activeInput];
            }
            Console.Clear();
            return FileName;
        }
        /// <summary>Загрузка из сериализованного файла объекта змейки, Если файл не найден - будет сгенерировано исключение</summary>
        /// <param name="File">Имя сериализованного файла объекта змейки</param>
        /// <returns></returns>
        public static void Load(string FileName, ref Snake snake)
        {   
            if (FileName.Length==0) return;
            BinaryFormatter formatter = new BinaryFormatter();
            int y=(int) Console.WindowHeight/2;
            int x=(int) Console.WindowWidth/2;
            if (File.Exists(FileName)) 
            {
                using (FileStream fs = new FileStream(FileName, FileMode.OpenOrCreate)) 
                {
                    snake = (Snake) formatter.Deserialize(fs);
                }
                Console.SetCursorPosition(x-12,y-2); Console.Write( "╔═══════════════════════╗");
                Console.SetCursorPosition(x-12,y-1); Console.Write( "║    Файл прочитан!     ║");
                Console.SetCursorPosition(x-12,y-0); Console.Write( "║ Объект десериализован ║");
                Console.SetCursorPosition(x-12,y+1); Console.Write( "║ Нажмите любую клавишу ║");
                Console.SetCursorPosition(x-12,y+2); Console.Write( "╚═══════════════════════╝"); 
                Console.ReadKey();
            }
            else
            {
                Console.SetCursorPosition(x-12,y-2); Console.Write( "╔═══════════════════════╗");
                Console.SetCursorPosition(x-12,y-1); Console.Write( "║    Файл не найден !   ║");
                Console.SetCursorPosition(x-12,y-0); Console.Write( "║                       ║");
                Console.SetCursorPosition(x-12,y+1); Console.Write( "║ Нажмите любую клавишу ║");
                Console.SetCursorPosition(x-12,y+2); Console.Write( "╚═══════════════════════╝"); 
                Console.ReadKey();
            }
        }
        public static void Save(string FileName, ref Snake snake)
        { 
            if (FileName.Length==0) return;  
            int y=(int) Console.WindowHeight/2;
            int x=(int) Console.WindowWidth/2;
            Console.SetCursorPosition(8, 4*11+6);            
            // создаем объект BinaryFormatter
            BinaryFormatter formatter = new BinaryFormatter();
            // получаем поток, куда будем записывать сериализованный объект
  
                if (File.Exists(FileName)) 
                {

                    Console.SetCursorPosition(x-12,y-2); Console.Write( "╔═══════════════════════╗");
                    Console.SetCursorPosition(x-12,y-1); Console.Write( "║   Файл существует!    ║");
                    Console.SetCursorPosition(x-12,y-0); Console.Write( "║    Перезаписать ?     ║");
                    Console.SetCursorPosition(x-12,y+1); Console.Write( "║       (Y-Да)          ║");
                    Console.SetCursorPosition(x-12,y+2); Console.Write( "╚═══════════════════════╝");    
                    ConsoleKeyInfo key=Console.ReadKey();
                    if (key.Key==ConsoleKey.Y)
                    {
                        File.Delete(FileName);
                        using (FileStream fs = new FileStream(FileName, FileMode.OpenOrCreate)) { formatter.Serialize(fs, snake);}
                        Console.SetCursorPosition(x-12,y-1); Console.Write( "║  Объект сериализован  ║");
                        Console.SetCursorPosition(x-12,y-0); Console.Write( "║    Файл перезаписан   ║");
                        Console.SetCursorPosition(x-12,y+1); Console.Write( "║ Нажмите любую клавишу ║");
                        Console.ReadKey();
                        return;
                    }
                    else
                    {
                        Console.SetCursorPosition(x-12,y-1); Console.Write( "║                       ║");
                        Console.SetCursorPosition(x-12,y-0); Console.Write( "║     Отмена записи     ║");
                        Console.SetCursorPosition(x-12,y+1); Console.Write( "║ Нажмите любую клавишу ║");
                        Console.ReadKey();
                        return;
                    }
                }
                using (FileStream fs = new FileStream(FileName, FileMode.OpenOrCreate)) { formatter.Serialize(fs, snake);}
                Console.SetCursorPosition(x-12,y-2); Console.Write( "╔═══════════════════════╗");
                Console.SetCursorPosition(x-12,y-1); Console.Write( "║  Объект сериализован  ║");
                Console.SetCursorPosition(x-12,y-0); Console.Write( "║     Файл записан      ║");
                Console.SetCursorPosition(x-12,y+1); Console.Write( "║ Нажмите любую клавишу ║");
                Console.SetCursorPosition(x-12,y+2); Console.Write( "╚═══════════════════════╝"); 
                Console.ReadKey();
                return;
        }
    }
    /// <summary>Редактор специальной трёхмерной матрицы поведения змейки.</summary>
    class MatrixEditor
    {
        /// <summary>Отображает матрицу</summary>
        /// <param name="table">Массив с матрицей</param>
        /// <returns></returns>
        private static void showTable (sbyte[,,] table,int i=11, int j=11, int k=4)
        {   Console.Clear();
            string title = "Редактор матрицы поведения Змейки";
            Console.SetCursorPosition((Console.WindowWidth / 2) - (title.Length / 2), 1);Console.Write(title);
            title = "← ↑ → ↓ - Указать элемент; ↲ - Заполнить элемент";
            Console.SetCursorPosition((Console.WindowWidth / 2) - (title.Length / 2), Console.WindowHeight-4);Console.Write(title);
            title = "F10 - Загрузка; F12 - Сохранить; Esc - Выход (изменения будут сохранены)";
            Console.SetCursorPosition((Console.WindowWidth / 2) - (title.Length / 2), Console.WindowHeight-2);Console.Write(title);

            int x,y,shift=(Console.WindowWidth / 2) - (8*i / 2)+8;
            for (int ii=0;ii<i;ii++){ y=4*ii+6; Console.SetCursorPosition(shift-8,y); Console.Write($"{ii}");}
            for (int ij=0;ij<j;ij++) {x=8*ij+shift-3;Console.SetCursorPosition(x,3); Console.Write($"{ij}");}
            for (int ii=0;ii<i;ii++)
            {
                y=4*ii+6;
                for (int ij=0;ij<j;ij++)
                {
                    x=8*ij+shift;
                    Console.SetCursorPosition(x-7,y-2); Console.Write( "┼───────┼");
                    Console.SetCursorPosition(x-7,y-1); Console.Write( "│       │");
                    Console.SetCursorPosition(x-7,y-0); Console.Write( "│       │");
                    Console.SetCursorPosition(x-7,y+1); Console.Write( "│       │");
                    Console.SetCursorPosition(x-7,y+2); Console.Write( "┼───────┼");
                    Console.SetCursorPosition(x-6,y+0); Console.Write($"{table[ii,ij,0],3:###}");
                    Console.SetCursorPosition(x-5,y+1); Console.Write($"{table[ii,ij,1],3:###}");
                    Console.SetCursorPosition(x-5,y-1); Console.Write($"{table[ii,ij,2],3:###}");
                    Console.SetCursorPosition(x-2,y+0); Console.Write($"{table[ii,ij,3]:###}");
                }    
            }        
        }
        /// <summary>Главный цикл</summary>
        /// <param name="table">Массив с матрицей поведения</param>
        /// <param name="i">Ширина, целое, 11 по-умолчанию</param>
        /// <param name="j">Высота, целое, 11 по-умолчанию</param>
        public static void Loop(ref sbyte[,,] table,int i=11, int j=11)
        {
            bool loop = true;
            showTable (table);
            ConsoleKeyInfo cki;
            int x=5,y=5,lx=4,ly=4,cx,cy,shift=(Console.WindowWidth / 2) - (8*i / 2)+8;
            do 
            {   Console.CursorVisible = false;
                if (Console.KeyAvailable) 
                {
                    cki = Console.ReadKey(true);
                    switch (cki.Key)
                    {
                        case (ConsoleKey.LeftArrow) : x--; break;
                        case (ConsoleKey.DownArrow) : y++; break;
                        case (ConsoleKey.UpArrow)   : y--; break;
                        case (ConsoleKey.RightArrow): x++; break;
                        case (ConsoleKey.Enter)     : Edit(table,y,x);showTable (table);x++; break;
                        case (ConsoleKey.F12): Save(table,SnankeEditor.SelectFile("matrix"));showTable (table);x++; break;
                        case (ConsoleKey.F10): Load(ref table,SnankeEditor.SelectFile("matrix",false));showTable (table);x++; break;
                        case (ConsoleKey.Escape): loop=false; break;
                    }
                    if (x<0) x=i-1; if (y<0) y=j-1; if (x>i-1) x=0; if (y>j-1) y=0;

                }
                if (x!=lx || y!=ly)
                {
                    cx=8*lx+shift; cy=4*ly+6;
                    Console.SetCursorPosition(cx-7,cy-2); Console.Write( "┼───────┼"); 
                    Console.SetCursorPosition(cx-7,cy+2); Console.Write( "┼───────┼");    
                    cx=8*x+shift; cy=4*y+6;
                    Console.SetCursorPosition(cx-7,cy-2); Console.Write( "╔═══════╗");
                    Console.SetCursorPosition(cx-7,cy+2); Console.Write( "╚═══════╝");
               
                    lx=x; ly=y;
                    
                }

                    Thread.Sleep(15);
            } while(loop);
            Console.Clear();
        }
        /// <summary> Редактрирование ячейки. </summary>
        /// <param name="table">Массив с матрицей</param>
        /// <param name="i">позиция, целое</param>
        /// <param name="j">позиция, целое</param>
        private static void Edit (sbyte[,,] table,int i, int j)
        {   int cx=6*i+8, cy=4*j+6;
            string enter;
            int y=(int) Console.WindowHeight/2;
            int x=(int) Console.WindowWidth/2;
                    Console.CursorVisible = true;
                    Console.SetCursorPosition(x-12,y-2); Console.Write( "╔═══════════════════════╗");
                    Console.SetCursorPosition(x-12,y-1); Console.Write( "║    ВЕРХ =___          ║");
                    Console.SetCursorPosition(x-12,y-0); Console.Write( "║ ЛЕВО =___  ПРАВО=___  ║");
                    Console.SetCursorPosition(x-12,y+1); Console.Write( "║     НИЗ =___          ║");
                    Console.SetCursorPosition(x-12,y+2); Console.Write( "╚═══════════════════════╝");
                    Console.SetCursorPosition(x-4,y+0); Console.Write($"{table[i,j,0]}");
                    Console.SetCursorPosition(x-1,y+1); Console.Write($"{table[i,j,1]}");
                    Console.SetCursorPosition(x-1,y-1); Console.Write($"{table[i,j,2]}");
                    Console.SetCursorPosition(x+7,y+0); Console.Write($"{table[i,j,3]}");
                    Console.SetCursorPosition(x-4,y+0); enter=Console.ReadLine(); if (enter.Length>0) table[i,j,0]=sbyte.Parse(enter);   
                    Console.SetCursorPosition(x-1,y+1); enter=Console.ReadLine(); if (enter.Length>0) table[i,j,1]=sbyte.Parse(enter);
                    Console.SetCursorPosition(x-1,y-1); enter=Console.ReadLine(); if (enter.Length>0) table[i,j,2]=sbyte.Parse(enter);
                    Console.SetCursorPosition(x+7,y+0); enter=Console.ReadLine(); if (enter.Length>0) table[i,j,3]=sbyte.Parse(enter);
                    Console.CursorVisible = false;                
        }
        /// <summary>Сохранение в сериализованный файл https://docs.microsoft.com/ru-ru/dotnet/api/system.io.file.delete?view=net-6.0</summary>
        /// <param name="table">Массив с матрицей</param>
        /// <param name="FileName">Имя файла для сохранения</param>
        private static void Save (sbyte[,,] table, string FileName)
        {           
            if (FileName.Length==0) return;
            int y=(int) Console.WindowHeight/2;
            int x=(int) Console.WindowWidth/2;
            Console.SetCursorPosition(8, 4*11+6);            
            // создаем объект BinaryFormatter
            BinaryFormatter formatter = new BinaryFormatter();
            // получаем поток, куда будем записывать сериализованный объект
  
                if (File.Exists(FileName)) 
                {

                    Console.SetCursorPosition(x-12,y-2); Console.Write( "╔═══════════════════════╗");
                    Console.SetCursorPosition(x-12,y-1); Console.Write( "║   Файл существует!    ║");
                    Console.SetCursorPosition(x-12,y-0); Console.Write( "║    Перезаписать ?     ║");
                    Console.SetCursorPosition(x-12,y+1); Console.Write( "║       (Y-Да)          ║");
                    Console.SetCursorPosition(x-12,y+2); Console.Write( "╚═══════════════════════╝");    
                    ConsoleKeyInfo key=Console.ReadKey();
                    if (key.Key==ConsoleKey.Y)
                    {
                        File.Delete(FileName);
                        using (FileStream fs = new FileStream(FileName, FileMode.OpenOrCreate)) { formatter.Serialize(fs, table);}
                        Console.SetCursorPosition(x-12,y-1); Console.Write( "║  Объект сериализован  ║");
                        Console.SetCursorPosition(x-12,y-0); Console.Write( "║    Файл перезаписан   ║");
                        Console.SetCursorPosition(x-12,y+1); Console.Write( "║ Нажмите любую клавишу ║");
                        Console.ReadKey();
                        return;
                    }
                    else
                    {
                        Console.SetCursorPosition(x-12,y-1); Console.Write( "║                       ║");
                        Console.SetCursorPosition(x-12,y-0); Console.Write( "║     Отмена записи     ║");
                        Console.SetCursorPosition(x-12,y+1); Console.Write( "║ Нажмите любую клавишу ║");
                        Console.ReadKey();
                        return;
                    }
                }
                using (FileStream fs = new FileStream(FileName, FileMode.OpenOrCreate)) { formatter.Serialize(fs, table);}
                Console.SetCursorPosition(x-12,y-2); Console.Write( "╔═══════════════════════╗");
                Console.SetCursorPosition(x-12,y-1); Console.Write( "║  Объект сериализован  ║");
                Console.SetCursorPosition(x-12,y-0); Console.Write( "║     Файл записан      ║");
                Console.SetCursorPosition(x-12,y+1); Console.Write( "║ Нажмите любую клавишу ║");
                Console.SetCursorPosition(x-12,y+2); Console.Write( "╚═══════════════════════╝"); 
                Console.ReadKey();
                return;
        }
        /// <summary>Чтение из сериализованного файла. https://docs.microsoft.com/ru-ru/dotnet/api/system.io.file.delete?view=net-6.0</summary>
        /// <param name="table">Массив с матрицей, ссылка</param>
        /// <param name="FileName">Имя файла для сохранения</param>
        private static void Load (ref sbyte[,,] table, string FileName)
        {   
            if (FileName.Length==0) return;
            BinaryFormatter formatter = new BinaryFormatter();
            int y=(int) Console.WindowHeight/2;
            int x=(int) Console.WindowWidth/2;
            if (File.Exists(FileName)) 
            {
                using (FileStream fs = new FileStream(FileName, FileMode.OpenOrCreate)) 
                {
                    table = (sbyte[,,]) formatter.Deserialize(fs);
                }
                Console.SetCursorPosition(x-12,y-2); Console.Write( "╔═══════════════════════╗");
                Console.SetCursorPosition(x-12,y-1); Console.Write( "║    Файл прочитан!     ║");
                Console.SetCursorPosition(x-12,y-0); Console.Write( "║ Объект десериализован ║");
                Console.SetCursorPosition(x-12,y+1); Console.Write( "║ Нажмите любую клавишу ║");
                Console.SetCursorPosition(x-12,y+2); Console.Write( "╚═══════════════════════╝"); 
                Console.ReadKey();
            }
            else
            {
                Console.SetCursorPosition(x-12,y-2); Console.Write( "╔═══════════════════════╗");
                Console.SetCursorPosition(x-12,y-1); Console.Write( "║    Файл не найден !   ║");
                Console.SetCursorPosition(x-12,y-0); Console.Write( "║                       ║");
                Console.SetCursorPosition(x-12,y+1); Console.Write( "║ Нажмите любую клавишу ║");
                Console.SetCursorPosition(x-12,y+2); Console.Write( "╚═══════════════════════╝"); 
                Console.ReadKey();
            }
        }
    }
    ///<summary>Редактор параметров "мира".</summary>
    class worldEditor
    {
        ///<summary>Меню параметров "мира"</summary>
        /// <param name="FirstSnake">Первичная змейка</param>
        /// <param name="countOfEpoch">Количество эпох для отбора, 0 - бесконечно</param>
        /// <param name="periodOfSelection">Длительность одной эпохи отбора</param>
        /// <param name="countOfApples">Количество "яблок" в "мире"</param>
        public static void Setup (in Snake FirstSnake, out int countOfEpoch, out long periodOfSelection,out int countOfApples)
        {

            Console.Clear();

            string label = "Настройки мира";
            Console.SetCursorPosition((Console.WindowWidth / 2) - (label.Length / 2), 1);Console.Write(label);
            label = "↑ ↓ - Указать элемент; ↲ - Изменить элемент";
            Console.SetCursorPosition((Console.WindowWidth / 2) - (label.Length / 2), Console.WindowHeight-4);Console.Write(label);
            label = "Esc - Продолжить";
            Console.SetCursorPosition((Console.WindowWidth / 2) - (label.Length / 2), Console.WindowHeight-2);Console.Write(label);


            countOfApples=159;
            periodOfSelection=100000;
            countOfEpoch=0;

            bool loop = true;
            int activeInput = 0;
            List<Object> Sheet= new List<Object> (){countOfApples,periodOfSelection,countOfEpoch,FirstSnake};

            ConsoleKeyInfo cki;
            do 
            {   Console.CursorVisible = false;
                if (Console.KeyAvailable) 
                {
                    cki = Console.ReadKey(true);
                    switch (cki.Key)
                    {
                        case (ConsoleKey.DownArrow) : activeInput++; break;
                        case (ConsoleKey.UpArrow)   : activeInput--; break;
                        case (ConsoleKey.Enter)     : break;
                        case (ConsoleKey.Escape)    : loop=false; break;
                    }
                    if (activeInput<0) activeInput=Sheet.Count-1; if (activeInput>Sheet.Count-1) activeInput=0;
                }
                    Thread.Sleep(15);
            } while(loop);
            Console.Clear();


            return;
        } 
    }
        
}
    