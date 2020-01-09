

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PRM_JC_PP
{
    public partial class Form1 : Form
    {
        Bitmap image1 = new Bitmap(@" C:\WORLDMAP\worldmap.png", true);

        Pen bluePen = new Pen(Color.FromArgb(0,0,255));
        Pen redPen = new Pen(Color.FromArgb(255,0,0));
        Pen greenPen = new Pen(Color.FromArgb(0,255,0));

        private Points points;
        bool is_road;

        public struct Point
        {
            public int x;
            public int y;
            public List<int> neighboors;
            public List<int> distances;
            public Point(int px, int py)
            {
                x = px;
                y = py;
                neighboors = new List<int>();
                distances = new List<int>();
            }
        }

        public struct Points
        {
            public Point[] point;
            public int size;
            public Points(int value)
            {
                point = new Point[value + 2];
                size = 0;
            }
            public void add_point(int x, int y)
            {
                point[size++] = new Point(x, y);
            }
            public void add_first(int x, int y)      // Tokio
            {
                point[0] = new Point(x, y);
            }
            public void add_last(int x, int y)      // Londyn 
            {
                point[1] = new Point(x, y);
            }
        }
    
        public int distance_between_points(Point a, Point b)
        {
            float distance_between_x = Math.Abs(a.x - b.x);                           // wartosci bezwzgledne z odleglosci między konkretnymi współrzędnymi punktów, aby nie wartości nie były ujemne
            float distance_between_y = Math.Abs(a.y - b.y);
            int distance = (int)Math.Sqrt(distance_between_x * distance_between_x + distance_between_y * distance_between_y);   // odleglosc liczona jest z twierdzenia pitagorasa 
            return distance;
        }       // Wyliczenie dystansu między punktami
        
        private void Choose_points(object sender, EventArgs e)
        {
            int x;
            int y;

            Graphics g = Graphics.FromImage(image1);

            int Amount_of_points = int.Parse(textBox1.Text);    // ilość punktów do wylosowania i rozlokowania na mapie    
            this.points = new Points(Amount_of_points + 1);

            Random rnd = new Random();
            for (int i = 0; i < Amount_of_points; ++i)
            {
                x = rnd.Next(1, 3592);
                y = rnd.Next(1, 2416);

                if (image1.GetPixel(x, y).G != 0)           // Sprawdzenie które punkty znajduja się na wodzie i zaznaczenie ich kolorem 
                {
                    this.points.add_point(x, y);
                    SolidBrush myBrush;
                    myBrush = new SolidBrush(Color.FromArgb(0, 0, 255));
                    g.FillEllipse(myBrush, new Rectangle(x - 10, y - 10, 15, 15)); //rysowanie wierzchołków i ustawienie grubości linii 
                }
                else                                      // Sprawdzenie które punkty znajduja się na lądzie ( a dokładniej, które nie są na wodzie) i zaznaczenie ich kolorem żółtym  
                {
                    SolidBrush myBrush3;
                    myBrush3 = new SolidBrush(Color.FromArgb(250, 237, 39));
                    g.FillEllipse(myBrush3, new Rectangle(x - 10, y - 10, 15, 15));
                }
            }

            this.points.add_first(3060, 775);                                       //  Przypisanie wartosci dla Tokyo i Londynu oraz 
            SolidBrush myBrush1 = new SolidBrush(Color.FromArgb(255, 0, 0));        //  wybranie im koloru na mapie  
            g.FillEllipse(myBrush1, new Rectangle(3060 - 10, 765 - 10, 20, 20));

            this.points.add_last(1795, 541);
            SolidBrush myBrush2 = new SolidBrush(Color.FromArgb(0, 255, 0));
            g.FillEllipse(myBrush2, new Rectangle(1795, 541, 20, 20));

            Draw_points();
            label1.Text = "Wylosowano punkty";
        }     // Losowanie punktów

        private void Draw_points()
        {
            using (var graphics = Graphics.FromImage(image1))
            {
                pictureBox1.Image = image1;
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            }
        }                                 // Narysowanie wszystkich wybranych punktów
     
        private void PRM_method(object sender, EventArgs e) //Znalezienie losowych puntków na wodzie i narysowanie linii między który mi nie wystapi kolizja z lądem
        {
            int max_dist_between_points = int.Parse(textBox2.Text);
            for (int i = 0; i < this.points.size - 1; ++i)
            {
                for (int j = i + 1; j < this.points.size; ++j)
                {
                    Point a = this.points.point[i];
                    Point b = this.points.point[j];
                    is_road = false;
                    int dist = distance_between_points(a, b);

                    if (dist != 0 && dist < max_dist_between_points)
                    {
                        is_road = true;
                        land_detection(a, b);
                        if (is_road)
                        {
                            a = points_update(i, j, a, b, dist);
                            Draw_line(a, b);
                        }
                    }
                }
            }
            label1.Text = "Zakończono rysowanie";
        }

        public void land_detection(Point a, Point b)                    // wykrycie lądu pomiędzy połączonymi punktami. 
        {
            int width = b.x - a.x;
            int height = b.y - a.y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (width < 0) dx1 = -1; else if (width > 0) dx1 = 1;
            if (height < 0) dy1 = -1; else if (height > 0) dy1 = 1;
            if (width < 0) dx2 = -1; else if (width > 0) dx2 = 1;
            int longest = Math.Abs(width);
            int shortest = Math.Abs(height);
            if (longest < shortest)
            {
                longest = Math.Abs(height);
                shortest = Math.Abs(width);
                if (height < 0) dy2 = -1; else if (height > 0) dy2 = 1;
                dx2 = 0;
            }
            int counter = longest >> 1;
            for (int i = 0; i <= longest; i++)
            {
                if (image1.GetPixel(a.x, a.y).B == 0)
                {
                    is_road = false;
                    break;
                }
                counter += shortest;
                if (!(counter < longest))
                {
                    counter -= longest;
                    a.x += dx1;
                    a.y += dy1;
                }
                else
                {
                    a.x += dx2;
                    a.y += dy2;
                }
            }
        }

        private Point points_update(int i, int j, Point a, Point b, int dista)
        {
            a.neighboors.Add(j);
            b.neighboors.Add(i);
            a.distances.Add(dista);
            b.distances.Add(dista);
            a = this.points.point[i];
            return a;
        }       // Uaktualnienie punktów pomiędzy sobą - sąsiedzi, odległości

        private void Draw_line(Point a, Point b)
        {
            using (var graphics = Graphics.FromImage(image1))
            {
                graphics.DrawLine(bluePen, a.x, a.y, b.x, b.y);
                pictureBox1.Image = image1;
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            }
        }        // Rysowanie linii     

        private void Find_the_way(Object sender, EventArgs e)
        {
            Graphics g = Graphics.FromImage(image1);

            List<int> checkedd = new List<int>();
            List<int> to_check = new List<int>();
            for (int i = 0; i < this.points.size; i++)
            {
                to_check.Add(i);
            }

            List<int> distances = new List<int>();
            List<int> predecessor = new List<int>();
            for (int i = 0; i < this.points.size; i++)
            {
                distances.Add(1000000);
                predecessor.Add(-1);
            }
            distances[0] = 0;
            int how_much_to_check = to_check.Count;

            while (how_much_to_check > 0)
            {
                int minCost = 1000000;
                List<int> minimum = new List<int>();

                for (int i = 0; i < this.points.size; i++)
                {
                    if (to_check.Contains(i) && distances[i] < minCost)
                    {
                        minimum = new List<int>();
                        minimum.Add(i);
                        minCost = distances[i];
                    }
                    else if (to_check.Contains(i) && distances[i] == minCost)
                    {
                        minimum.Add(i);
                    }
                }

                for (int i = 0; i < minimum.Count; i++)
                {
                    checkedd.Add(minimum[i]);
                    to_check.Remove(minimum[i]);
                    how_much_to_check = to_check.Count;

                    var checked_vertex = minimum[i];

                    for (int k = 0; k < this.points.point[checked_vertex].neighboors.Count; k++)
                    {
                        if (to_check.Contains(this.points.point[checked_vertex].neighboors[k]))
                        {
                            if (distances[this.points.point[checked_vertex].neighboors[k]] > distances[checked_vertex] + this.points.point[checked_vertex].distances[k])
                            {
                                distances[this.points.point[checked_vertex].neighboors[k]] = distances[checked_vertex] + this.points.point[checked_vertex].distances[k];
                                predecessor[this.points.point[checked_vertex].neighboors[k]] = checked_vertex;
                            }
                        }
                    }
                }
            }

            List<int> road = new List<int>();
            road.Add(predecessor[1]);

            int first = predecessor[1];
            int counter = 0;

            while (first != 0)
            {
                try
                {
                    road.Add(predecessor[road[counter]]);
                    first = road[counter];
                    counter++;
                    label1.Text = "Najkrótsza droga pomiędzy Tokio, a Londynem";
                }
                catch (ArgumentOutOfRangeException outOfRange)
                {
                   label1.Text = "NIE MOŻNA WYZNACZYC DROGI - ZACZNIJ OD NOWA";
                }
            }
            Draw_dikstry(g, road);
        }       // Algorytm szukania optymalnej ścieżki 

        private void Draw_dikstry(Graphics g, List<int> droga)
        {
            for (int i = 0; i < this.points.size; i++)
            {
                if (droga.Contains(i))
                {
                    using (var graphics = Graphics.FromImage(image1))
                    {

                        SolidBrush myBrush1 = new SolidBrush(Color.FromArgb(255, 0, 0));
                        g.FillEllipse(myBrush1, new Rectangle(this.points.point[i].x - 10, this.points.point[i].y - 10, 30, 30));
                        pictureBox1.Image = image1;
                        pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    }
                }
            }
        }                  // Narysowanie znalezionej ścieżki

        public Form1()
        {
            InitializeComponent();
            //Set the PictureBox to display the image.
            pictureBox1.Image = image1;
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
        }
    }
}
