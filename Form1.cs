using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 五子棋
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();            
        }

        Pen pen = new Pen(Color.Black, 1);//用来画棋盘线
        Pen clear = new Pen(Color.BurlyWood);//用来清除棋子，即悔棋
        SolidBrush black = new SolidBrush(Color.Black);//用来画黑棋
        SolidBrush white = new SolidBrush(Color.White);//用来画白棋
        SolidBrush board = new SolidBrush(Color.BurlyWood);//用来画棋盘底色
        Random random = new Random();//用来设置随机数
        bool pvp = false, pve = false;//判断进行双人或者人机对战的布尔变量
        bool is_first;//判断是否用户先手的布尔变量
        int grid_size = 33;//棋盘格子大小
        int grid_number = 18;//棋盘格子数目
        int black_win_times = 0;//黑棋赢的次数
        int white_win_times = 0;//白棋赢的次数
        int time = 0;//本局用时        
        int[,] checkerboard = new int[19, 19];
        //储存棋盘的数组。checkerboard[x,y]的值为-1时，在(x,y)点的棋子是白子。值为0时，在(x,y)点没有棋子。值为1时，在(x,y)点的棋子是黑子        
        int[,] weight = new int[19, 19];//设置每一点的权重。在人机对战中，电脑下棋只会找权重最高的点下棋
        int[,] target = new int[10000 + 1, 2];//储存电脑下棋的所有目标位置的数组。通过权重筛选出最高权重的位置(x,y)，并储存到该数组中
        int n;//电脑下的是target中的第n列储存的点
        int checker_count = 0;//棋盘上所有棋子的数目
        int X, Y;//判断鼠标点击后下棋的位置(x,y)

        //某点某方向不堵对手两步就要赢
        bool is_must(int x, int y, int a, int b, int z)
        {
            bool flag = true;
            //活三
            for (int i = 1; i <= 3; i++)
            {
                if (x + i * a < 0 || x + i * a > grid_number || y + i * b < 0 || y + i * b > grid_number || checkerboard[x + i * a, y + i * b] != -z)
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                if ((x + 4 * a >= 0 && x + 4 * a <= grid_number && y + 4 * b >= 0 && y + 4 * b <= grid_number) && checkerboard[x + 4 * a, y + 4 * b] == 0)
                { return true; }
            }
            flag = true;
            //反向活三
            for (int i = 1; i <= 3; i++)
            {
                if (x - i * a < 0 || x - i * a > grid_number || y - i * b < 0 || y - i * b > grid_number || checkerboard[x - i * a, y - i * b] != -z )
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                if ((x - 4 * a >= 0 && x - 4 * a <= grid_number && y - 4 * b >= 0 && y - 4 * b <= grid_number) && checkerboard[x - 4 * a, y - 4 * b] == 0)
                { return true; }
            }
            flag = true;
            //左二右一
            for (int i = 1; i <= 2; i++)
            {
                if (x + i * a < 0 || x + i * a > grid_number || y + i * b < 0 || y + i * b > grid_number || checkerboard[x + i * a, y + i * b] != -z)
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                if ((x + 3 * a >= 0 && x + 3 * a <= grid_number && y + 3 * b >= 0 && y + 3 * b <= grid_number) && checkerboard[x + 3 * a, y + 3 * b] == 0)
                {
                    if ((x - a >= 0 && x - a <= grid_number && y - b >= 0 && y - b <= grid_number) && checkerboard[x - a, y - b] == -z)
                    {
                        if ((x - 2 * a >= 0 && x - 2 * a <= grid_number && y - 2 * b >= 0 && y - 2 * b <= grid_number) && checkerboard[x - 2 * a, y - 2 * b] == 0)
                        { return true; }
                    }
                }               
            }
            flag = true;
            //左一右二
            for (int i = 1; i <= 2; i++)
            {
                if (x - i * a < 0 || x - i * a > grid_number || y - i * b < 0 || y - i * b > grid_number || checkerboard[x - i * a, y - i * b] != -z)
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                if ((x - 3 * a >= 0 && x - 3 * a <= grid_number && y - 3 * b >= 0 && y - 3 * b <= grid_number) && checkerboard[x - 3 * a, y - 3 * b] != z)
                {
                    if ((x + a >= 0 && x + a <= grid_number && y + b >= 0 && y + b <= grid_number) && checkerboard[x + a, y + b] == -z)
                    {
                        if ((x + 2 * a >= 0 && x + 2 * a <= grid_number && y + 2 * b >= 0 && y + 2 * b <= grid_number) && checkerboard[x + 2 * a, y + 2 * b] != z)
                        { return true; }
                    }
                }
            }
            flag = true;            
            return false;
        }

        //某点不堵对手两步就要赢
        bool is_must(int x,int y,int self)
        {
            if(is_must(x, y, 1, 1, self)||
            is_must(x, y, 0, 1, self)||
            is_must(x, y, 1, -1, self)||
            is_must(x, y, 1, 0, self))
            { return true; }
            //判断对手是否形成正或者斜十字
            if (is_cross(x, y, -self)) { return true; }
            //判断对手是否成为斜T字
            if (is_T(x, y, -self)) { return true; }
            return false; 
        }

        //在如果下在该点上，则在某方向上必赢
        bool is_win(int x,int y,int a,int b,int self)
        {
            //要成五子
            int n1 = 0, n2 = 0;
            for (n1 = 1; n1 <= 4; n1++)
            {
                if (x + a * n1 >= 0 && x + a * n1 <= grid_number && y + b * n1 >= 0 && y + b * n1 <= grid_number && checkerboard[x + a * n1, y + b * n1] == self) 
                { continue; }
                else { break; }                
            }
            for (n2 = 1; n2 <= 4; n2++)
            {
                if (x - a * n2 >= 0 && x - a * n2 <= grid_number && y - b * n2 >= 0 && y - b * n2 <= grid_number && checkerboard[x - a * n2, y - b * n2] == self)
                { continue; }
                else { break; }
            }
            if (n1 + n2 >= 6) { return true; }
            else { return false; }
        }

        //下在该点必赢情况
        bool is_win(int x,int y,int checker)
        {
            if (is_win(x, y, -1, -1, checker) || is_win(x, y, 0, -1, checker) || is_win(x, y, 1, -1, checker) || is_win(x, y, -1, 0, checker))
            { return true; }
            else { return false; }
        }

        //判断如果下在该点上，则某方向上自己下一步必赢
        bool is_will_win(int x,int y,int a,int b,int self)
        {
            //能成活四、活五           
            int n1, n2;
            for (n1 = 1; n1 <= 4; n1++)
            {
                if (x + a * n1 >= 0 && x + a * n1 <= grid_number && y + b * n1 >= 0 && y + b * n1 <= grid_number && checkerboard[x + a * n1, y + b * n1] == self)
                { continue; }
                else { break; }
            }            
            for (n2 = 1; n2 <= 4; n2++)
            {
                if (x - a * n2 >= 0 && x - a * n2 <= grid_number && y - b * n2 >= 0 && y - b * n2 <= grid_number && checkerboard[x - a * n2, y - b * n2] == self)
                { continue; }
                else { break; }
            }
            if (n1 + n2 >= 5 && x + a * n1 >= 0 && x + a * n1 <= grid_number && y + b * n1 >= 0 && y + b * n1 <= grid_number && checkerboard[x + a * n1, y + b * n1] == 0 && x - n2 * a >= 0 && x - n2 * a <= grid_number && y - n2 * b >= 0 && y - n2 * b <= grid_number && checkerboard[x - n2 * a, y - n2 * b] == 0) 
            { return true; }            
            return false; 
        }

        //判断如果下在该点上，则自己下一步必赢
        bool is_will_win(int x,int y,int self)
        {
            //能成T字
            if (is_T(x, y, self)) { return true; }
            //能成双活二
            if (is_double(x, y, self)) { return true; }
            //能成正或斜十字
            if (is_cross(x, y, self)) { return true; }
            //能成活四或活五
            if (is_will_win(x, y, -1, -1, self) || is_will_win(x, y, 0, -1, self) || is_will_win(x, y, 1, -1, self) || is_will_win(x, y, -1, 0, self))
            { return true; }
            return false;
        }

        //判断某点是能成正或斜十字
        bool is_cross(int x,int y,int self) 
        {
            if (x - 2 >= 0 && x + 2 <= grid_number && y - 2 >= 0 && y + 2 <= grid_number)
            {
                if (checkerboard[x - 1, y] == self && checkerboard[x + 1, y] == self && checkerboard[x, y - 1] == self && checkerboard[x, y + 1] == self && checkerboard[x, y + 2] != -self && checkerboard[x, y - 2] != -self && checkerboard[x - 2, y] != -self && checkerboard[x + 2, y] != -self)
                { return true; }
                if (checkerboard[x - 1, y - 1] == self && checkerboard[x + 1, y + 1] == self && checkerboard[x + 1, y - 1] == self && checkerboard[x - 1, y + 1] == self && checkerboard[x + 2, y + 2] != -self && checkerboard[x - 2, y - 2] != self && checkerboard[x + 2, y - 2] != self && checkerboard[x - 2, y + 2] != self)
                { return true; }
            }
            return false;
        }

        //判断某点在某方向上是否有斜T字
        bool is_T(int x, int y, int a, int b, int z)
        {
            if (a == b)
            {
                //1方向上的斜T字
                if (x + 1 >= 0 && x + 1 <= grid_number && y + 1 >= 0 && y + 1 <= grid_number && checkerboard[x + 1, y + 1] == z)
                {
                    if (x + 2 >= 0 && x + 2 <= grid_number && y + 2 >= 0 && y + 2 <= grid_number && checkerboard[x + 2, y + 2] == z)
                    {
                        if (x + 3 >= 0 && x + 3 <= grid_number && y + 3 >= 0 && y + 3 <= grid_number && checkerboard[x + 3, y + 3] == 0)
                        {
                            if (x - 1 >= 0 && x - 1 <= grid_number && y + 1 >= 0 && y + 1 <= grid_number && checkerboard[x - 1, y + 1] == z)
                            {
                                if (x - 2 >= 0 && x - 2 <= grid_number && y + 2 >= 0 && y + 2 <= grid_number && checkerboard[x - 2, y + 2] == 0)
                                {
                                    if (x + 1 >= 0 && x + 1 <= grid_number && y - 1 >= 0 && y - 1 <= grid_number && checkerboard[x + 1, y - 1] == z)
                                    {
                                        if (x + 2 >= 0 && x + 2 <= grid_number && y - 2 >= 0 && y - 2 <= grid_number && checkerboard[x + 2, y - 2] == 0)
                                        { return true; }
                                    }
                                }
                            }
                        }
                    }
                }
                //8方向上的斜T字
                if (x - 1 >= 0 && x - 1 <= grid_number && y - 1 >= 0 && y - 1 <= grid_number && checkerboard[x - 1, y - 1] == z)
                {
                    if (x - 2 >= 0 && x - 2 <= grid_number && y - 2 >= 0 && y - 2 <= grid_number && checkerboard[x - 2, y - 2] == z)
                    {
                        if (x - 3 >= 0 && x - 3 <= grid_number && y - 3 >= 0 && y - 3 <= grid_number && checkerboard[x - 3, y - 3] == 0)
                        {
                            if (x - 1 >= 0 && x - 1 <= grid_number && y + 1 >= 0 && y + 1 <= grid_number && checkerboard[x - 1, y + 1] == z)
                            {
                                if (x - 2 >= 0 && x - 2 <= grid_number && y + 2 >= 0 && y + 2 <= grid_number && checkerboard[x - 2, y + 2] == 0)
                                {
                                    if (x + 1 >= 0 && x + 1 <= grid_number && y - 1 >= 0 && y - 1 <= grid_number && checkerboard[x + 1, y - 1] == z)
                                    {
                                        if (x + 2 >= 0 && x + 2 <= grid_number && y - 2 >= 0 && y - 2 <= grid_number && checkerboard[x + 2, y - 2] == 0)
                                        { return true; }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (a == 0)
            {
                //2方向上的斜T字
                if (x >= 0 && x <= grid_number && y - 1 >= 0 && y - 1 <= grid_number && checkerboard[x, y - 1] == z)
                {
                    if (x >= 0 && x <= grid_number && y - 2 >= 0 && y - 2 <= grid_number && checkerboard[x, y - 2] == z)
                    {
                        if (x >= 0 && x <= grid_number && y - 3 >= 0 && y - 3 <= grid_number && checkerboard[x, y - 3] == 0)
                        {
                            if (x - 1 >= 0 && x - 1 <= grid_number && y >= 0 && y <= grid_number && checkerboard[x - 1, y] == z)
                            {
                                if (x - 2 >= 0 && x - 2 <= grid_number && y >= 0 && y <= grid_number && checkerboard[x - 2, y] == 0)
                                {
                                    if (x + 1 >= 0 && x + 1 <= grid_number && y >= 0 && y <= grid_number && checkerboard[x + 1, y] == z)
                                    {
                                        if (x + 2 >= 0 && x + 2 <= grid_number && y >= 0 && y <= grid_number && checkerboard[x + 2, y] == 0)
                                        { return true; }
                                    }
                                }
                            }
                        }
                    }
                }
                //7方向上的斜T字
                if (x >= 0 && x <= grid_number && y + 1 >= 0 && y + 1 <= grid_number && checkerboard[x, y + 1] == z)
                {
                    if (x >= 0 && x <= grid_number && y + 2 >= 0 && y + 2 <= grid_number && checkerboard[x, y + 2] == z)
                    {
                        if (x >= 0 && x <= grid_number && y + 3 >= 0 && y + 3 <= grid_number && checkerboard[x, y + 3] == 0)
                        {
                            if (x - 1 >= 0 && x - 1 <= grid_number && y >= 0 && y <= grid_number && checkerboard[x - 1, y] == z)
                            {
                                if (x - 2 >= 0 && x - 2 <= grid_number && y >= 0 && y <= grid_number && checkerboard[x - 2, y] == 0)
                                {
                                    if (x + 1 >= 0 && x + 1 <= grid_number && y >= 0 && y <= grid_number && checkerboard[x + 1, y] == z)
                                    {
                                        if (x + 2 >= 0 && x + 2 <= grid_number && y >= 0 && y <= grid_number && checkerboard[x + 2, y] == 0)
                                        { return true; }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (a == -b)
            {
                //3方向上的斜T字
                if (x + 1 >= 0 && x + 1 <= grid_number && y - 1 >= 0 && y - 1 <= grid_number && checkerboard[x + 1, y - 1] == z)
                {
                    if (x + 2 >= 0 && x + 2 <= grid_number && y - 2 >= 0 && y - 2 <= grid_number && checkerboard[x + 2, y - 2] == z)
                    {
                        if (x + 3 >= 0 && x + 3 <= grid_number && y - 3 >= 0 && y - 3 <= grid_number && checkerboard[x + 3, y - 3] == 0)
                        {
                            if (x - 1 >= 0 && x - 1 <= grid_number && y - 1 >= 0 && y - 1 <= grid_number && checkerboard[x - 1, y - 1] == z)
                            {
                                if (x - 2 >= 0 && x - 2 <= grid_number && y - 2 >= 0 && y - 2 <= grid_number && checkerboard[x - 2, y - 2] == 0)
                                {
                                    if (x + 1 >= 0 && x + 1 <= grid_number && y + 1 >= 0 && y + 1 <= grid_number && checkerboard[x + 1, y + 1] == z)
                                    {
                                        if (x + 2 >= 0 && x + 2 <= grid_number && y + 2 >= 0 && y + 2 <= grid_number && checkerboard[x + 2, y + 2] == 0)
                                        { return true; }
                                    }
                                }
                            }
                        }
                    }
                }
                //6方向上的斜T字
                if (x + 1 >= 0 && x + 1 <= grid_number && y - 1 >= 0 && y - 1 <= grid_number && checkerboard[x + 1, y - 1] == z)
                {
                    if (x + 2 >= 0 && x + 2 <= grid_number && y - 2 >= 0 && y - 2 <= grid_number && checkerboard[x + 2, y - 2] == z)
                    {
                        if (x + 3 >= 0 && x + 3 <= grid_number && y - 3 >= 0 && y - 3 <= grid_number && checkerboard[x + 3, y - 3] == 0)
                        {
                            if (x - 1 >= 0 && x - 1 <= grid_number && y - 1 >= 0 && y - 1 <= grid_number && checkerboard[x - 1, y - 1] == z)
                            {
                                if (x - 2 >= 0 && x - 2 <= grid_number && y - 2 >= 0 && y - 2 <= grid_number && checkerboard[x - 2, y - 2] == 0)
                                {
                                    if (x + 1 >= 0 && x + 1 <= grid_number && y + 1 >= 0 && y + 1 <= grid_number && checkerboard[x + 1, y + 1] == z)
                                    {
                                        if (x + 2 >= 0 && x + 2 <= grid_number && y + 2 >= 0 && y + 2 <= grid_number && checkerboard[x + 2, y + 2] == 0)
                                        { return true; }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (b == 0)
            {
                //4方向上的斜T字
                if (x - 1 >= 0 && x - 1 <= grid_number && y >= 0 && y <= grid_number && checkerboard[x - 1, y] == z)
                {
                    if (x - 2 >= 0 && x - 2 <= grid_number && y >= 0 && y <= grid_number && checkerboard[x - 2, y] == z)
                    {
                        if (x - 3 >= 0 && x - 3 <= grid_number && y >= 0 && y <= grid_number && checkerboard[x - 3, y] == 0)
                        {
                            if (x >= 0 && x <= grid_number && y - 1 >= 0 && y - 1 <= grid_number && checkerboard[x, y - 1] == z)
                            {
                                if (x >= 0 && x <= grid_number && y - 2 >= 0 && y - 2 <= grid_number && checkerboard[x, y - 2] == 0)
                                {
                                    if (x >= 0 && x <= grid_number && y + 1 >= 0 && y + 1 <= grid_number && checkerboard[x, y + 1] == z)
                                    {
                                        if (x >= 0 && x <= grid_number && y + 2 >= 0 && y + 2 <= grid_number && checkerboard[x, y + 2] == 0)
                                        { return true; }
                                    }
                                }
                            }
                        }
                    }
                }
                //6方向上的斜T字
                if (x + 1 >= 0 && x + 1 <= grid_number && y >= 0 && y <= grid_number && checkerboard[x + 1, y] == z)
                {
                    if (x + 2 >= 0 && x + 2 <= grid_number && y >= 0 && y <= grid_number && checkerboard[x + 2, y] == z)
                    {
                        if (x + 3 >= 0 && x + 3 <= grid_number && y >= 0 && y <= grid_number && checkerboard[x + 3, y] == 0)
                        {
                            if (x >= 0 && x <= grid_number && y - 1 >= 0 && y - 1 <= grid_number && checkerboard[x, y - 1] == z)
                            {
                                if (x >= 0 && x <= grid_number && y - 2 >= 0 && y - 2 <= grid_number && checkerboard[x, y - 2] == 0)
                                {
                                    if (x >= 0 && x <= grid_number && y + 1 >= 0 && y + 1 <= grid_number && checkerboard[x, y + 1] == z)
                                    {
                                        if (x >= 0 && x <= grid_number && y + 2 >= 0 && y + 2 <= grid_number && checkerboard[x, y + 2] == 0)
                                        { return true; }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        //判断某点是否有斜T字
        bool is_T(int x,int y,int self) 
        {
            if (is_T(x, y, -1, -1, self) || is_T(x, y, 0, -1, self) || is_T(x, y, 1, -1, self) || is_T(x, y, -1, 0, self))
            { return true; }
            return false;
        }

        //在某点的某方向上有双活二
        bool is_double(int x,int y,int a,int b,int checker)
        {            
            if (x + a >= 0 && x + a <= grid_number && y + b >= 0 && y + b <= grid_number && checkerboard[x + a, y + b] == checker)
            {
                if (x + 2 * a >= 0 && x + 2 * a <= grid_number && y + 2 * b >= 0 && y + 2 * b <= grid_number && checkerboard[x + 2 * a, y + 2 * b] == checker)                
                {
                    if ((x + 3 * a >= 0 && x + 3 * a <= grid_number && y + 3 * b >= 0 && y + 3 * b <= grid_number && checkerboard[x + 3 * a, y + 3 * b] != -checker )) 
                    {
                        if (a != b)
                        {
                            if (x - 1 >= 0 && x - 1 <= grid_number && y - 1 >= 0 && y - 1 <= grid_number && checkerboard[x - 1, y - 1] == checker)
                            {
                                if (x - 2 >= 0 && x - 2 <= grid_number && y - 2 >= 0 && y - 2 <= grid_number && checkerboard[x - 2, y - 2] == checker)
                                {
                                    if (x - 3 >= 0 && x - 3 <= grid_number && y - 3 >= 0 && y - 3 <= grid_number && checkerboard[x - 3, y - 3] != -checker)
                                    { return true; }
                                }
                            }
                        }
                        if (a != 0)
                        {
                            if (x >= 0 && x <= grid_number && y - 1 >= 0 && y - 1 <= grid_number && checkerboard[x, y - 1] == checker)
                            {
                                if (x >= 0 && x <= grid_number && y - 2 >= 0 && y - 2 <= grid_number && checkerboard[x, y - 2] == checker)
                                {
                                    if (x >= 0 && x <= grid_number && y - 3 >= 0 && y - 3 <= grid_number && checkerboard[x, y - 3] != -checker)
                                    { return true; }
                                }
                            }
                        }
                        if (a != -b)
                        {
                            if (x + 1 >= 0 && x + 1 <= grid_number && y - 1 >= 0 && y - 1 <= grid_number && checkerboard[x + 1, y - 1] == checker)
                            {
                                if (x + 2 >= 0 && x + 2 <= grid_number && y - 2 >= 0 && y - 2 <= grid_number && checkerboard[x + 2, y - 2] == checker)
                                {
                                    if (x + 3 >= 0 && x + 3 <= grid_number && y - 3 >= 0 && y - 3 <= grid_number && checkerboard[x + 3, y - 3] != -checker)
                                    { return true; }
                                }
                            }
                        }
                        if (b != 0)
                        {
                            if (x - 1 >= 0 && x - 1 <= grid_number && y >= 0 && y <= grid_number && checkerboard[x - 1, y] == checker)
                            {
                                if (x - 2 >= 0 && x - 2 <= grid_number && y >= 0 && y <= grid_number && checkerboard[x - 2, y] == checker)
                                {
                                    if (x - 3 >= 0 && x - 3 <= grid_number && y >= 0 && y <= grid_number && checkerboard[x - 3, y] != -checker)
                                    { return true; }
                                }
                            }
                        }
                        if (b != 0)
                        {
                            if (x + 1 >= 0 && x + 1 <= grid_number && y >= 0 && y <= grid_number && checkerboard[x + 1, y] == checker)
                            {
                                if (x + 2 >= 0 && x + 2 <= grid_number && y >= 0 && y <= grid_number && checkerboard[x + 2, y] == checker)
                                {
                                    if (x + 3 >= 0 && x + 3 <= grid_number && y >= 0 && y <= grid_number && checkerboard[x + 3, y] != -checker)
                                    { return true; }
                                }
                            }
                        }
                        if (a != -b)
                        {
                            if (x - 1 >= 0 && x - 1 <= grid_number && y + 1 >= 0 && y + 1 <= grid_number && checkerboard[x - 1, y + 1] == checker)
                            {
                                if (x - 2 >= 0 && x - 2 <= grid_number && y + 2 >= 0 && y + 2 <= grid_number && checkerboard[x - 2, y + 2] == checker)
                                {
                                    if (x - 3 >= 0 && x - 3 <= grid_number && y + 3 >= 0 && y + 3 <= grid_number && checkerboard[x - 3, y + 3] != -checker)
                                    { return true; }
                                }
                            }
                        }
                        if (a != 0)
                        {
                            if (x >= 0 && x <= grid_number && y + 1 >= 0 && y + 1 <= grid_number && checkerboard[x, y + 1] == checker)
                            {
                                if (x >= 0 && x <= grid_number && y + 2 >= 0 && y + 2 <= grid_number && checkerboard[x, y + 2] == checker)
                                {
                                    if (x >= 0 && x <= grid_number && y + 3 >= 0 && y + 3 <= grid_number && checkerboard[x, y + 3] != -checker)
                                    { return true; }
                                }
                            }
                        }
                        if (a != b)
                        {
                            if (x + 1 >= 0 && x + 1 <= grid_number && y + 1 >= 0 && y + 1 <= grid_number && checkerboard[x + 1, y + 1] == checker)
                            {
                                if (x + 2 >= 0 && x + 2 <= grid_number && y + 2 >= 0 && y + 2 <= grid_number && checkerboard[x + 2, y + 2] == checker)
                                {
                                    if (x + 3 >= 0 && x <= grid_number && y + 3 >= 0 && y + 3 <= grid_number && checkerboard[x + 3, y + 3] != -checker)
                                    { return true; }
                                }
                            }
                        }
                        return false;
                    }
                }
            }
            return false;
        }        

        //在某点有双活二
        bool is_double(int x,int y,int checker)
        {
            if(is_double(x,y,-1,-1,checker)|| is_double(x, y, 0, 1, checker)|| is_double(x, y, 1, -1, checker)|| is_double(x, y, -1, 0, checker)
            || is_double(x, y, 1, 0, checker)|| is_double(x, y, -1, 1, checker)|| is_double(x, y, 0, 1, checker)|| is_double(x, y, 1, 1, checker))
            { return true; }
            return false;
        }       

        //正常情况下设置某点在某方向上的权重
        void set_normal_weight(int x,int y,int a,int b)
        {
            if (x + a >= 0 && x + a <= grid_number && y + b >= 0 && y + b <= grid_number && checkerboard[x + a, y + b] != 0)
            {
                weight[x, y]++;
                for (int i = 2; i <= 4 && x + i * a <= grid_number && x + i * a >= 0 && y + i * b >= 0 && y + i * b <= grid_number; i++)
                {
                    if (checkerboard[x + i * a, y + i * b] == checkerboard[x + a, y + b]) { weight[x, y]++; }
                    if (checkerboard[x + i * a, y + i * b] == -checkerboard[x + a, y + b]) { weight[x, y]--; }
                    if (checkerboard[x + i * a, y + i * b] == 0) { break; }
                }
            }
        }

        //正常情况下设置某点的权重
        void set_normal_weight(int x,int y)
        {
            set_normal_weight(x, y, -1, -1);
            set_normal_weight(x, y, 0, -1);
            set_normal_weight(x, y, 1, -1);
            set_normal_weight(x, y, -1, 0);
            set_normal_weight(x, y, 1, 0);
            set_normal_weight(x, y, -1, 1);
            set_normal_weight(x, y, 0, 1);
            set_normal_weight(x, y, 1, 1);
            if (weight[x, y] < 0) { weight[x, y] = 0; }
        }

        //显示所有的权重
        void show_weight()
        {            
            Graphics g = CreateGraphics();            
            for(int i = 0; i <= grid_number; i++)
            {
                for(int j = 0; j <= grid_number; j++)
                {
                    if (weight[i, j] != 0)
                    {                        
                        g.DrawString(Convert.ToString(weight[i, j]), new Font("楷体", 8), black, 100 + i * grid_size, 50 + j * grid_size);
                    }
                }
            }
        }

        //设置棋子下的位置
        int checker_position(int a, int interval)
        {
            int result = -1;
            if (Math.Abs(a - interval) < grid_size / 3) { return 0; }
            else
            {
                for (int i = 0; i < grid_number; i++)
                {
                    if (Math.Abs(a - interval - grid_size * i) > Math.Abs(a - interval - (i + 1) * grid_size))
                        result = i + 1;
                }
                if (Math.Abs(a - interval - result * grid_size) < grid_size / 3) { return result; }
                else { return -1; }
            }
        }       

        //设置所有控件的可见性
        void set_visible(bool a)
        {
            if (a == false)
            {
                label1.Visible = false;
                label2.Visible = false;
                label3.Visible = false;
                label4.Visible = false;
                button1.Visible = true;
                button2.Visible = false;
                button3.Visible = true;
                menuStrip1.Visible = false;
            }
            if (a)
            {
                button1.Visible = false;
                button2.Visible = true;
                button3.Visible = false;
                menuStrip1.Visible = true;
                label1.Visible = true;
                label2.Visible = true;
                label3.Visible = true;
                label4.Visible = true;
            }
        }

        //设置所有按钮的位置
        void set_button_position()
        {
            Point[] point = new Point[2];
            point[0].X = Width / 2 - button1.Width / 2;
            point[0].Y = Height / 2 - button1.Height / 2 - 100;
            point[1].X = Width / 2 - button1.Width / 2;
            point[1].Y = Height / 2 - button1.Height / 2 + 100;
            button1.Location = point[0];
            button3.Location = point[1];
        }

        //设置所有标签的位置
        void set_lable_position()
        {
            Point[] point = new Point[5];
            for(int i = 0; i < 5; i++)
            {
                point[i].X = 150 + grid_size * grid_number;
                point[i].Y = 100 + i * 50;
            }
            label1.Location = point[0];
            label2.Location = point[1];
            label3.Location = point[2];
            label4.Location = point[3];
            button2.Location = point[4];
        }

        //初始化所有标签的内容
        void set_label()
        {
            set_visible(true);
            label1.Text = "当前回合：黑棋";
            label2.Text = "黑棋获胜次数：0";
            label3.Text = "白棋获胜次数：0";
            label4.Text = "用时：0:0:0";
            timer1.Start();
            Invalidate();
        }

        //设置某点的权重
        void set_weight(int x,int y,int self)
        {                        
            //判断自己下这个点能直接赢
            if (is_win(x, y, self)) { weight[x, y] = 10000; }
            //判断对手下一步下这里能直接赢
            if (weight[x, y] == 0)
            {
                if (is_win(x, y, -self)) { weight[x, y] = 9000; }
            }
            //判断自己这一步下这里，下步能直接赢
            if (weight[x, y] == 0)
            {
                if(is_will_win(x, y, self)) { weight[x, y] = 8000; }                          
            }            
            //判断对手双活二
            if (weight[x, y] == 0)
            { 
                if (is_double(x, y, -self)) { weight[x, y] = 7000; } 
            }
            //判断对手下一步下在该点,能下下一步必赢
            if (weight[x, y] == 0)
            {
                if (is_must(x, y, self)) { weight[x, y] = 6000; }            
            }
            //在平常情况下设置该点的权重
            if (weight[x, y] == 0)
            {
                set_normal_weight(x, y);
            }
        }

        //判断某点某方向上是否成五子
        string is_five(int i,int j,int a,int b)
        {
            if (checkerboard[i, j] != 0) 
            {
                for (int z = 1; z <= 4; z++)
                {
                    if (i + z * a > grid_number || i + z * a < 0 || j + z * b > grid_number || j + z * b < 0) { return null; }
                    if (checkerboard[i + z * a, j + z * b] != checkerboard[i, j]) { return null; }
                }
                timer1.Stop();
                if (checkerboard[i, j] == 1) 
                {
                    black_win_times++;
                    label2.Text = "黑棋获胜次数：" + black_win_times;
                    label3.Text = "白棋获胜次数：" + white_win_times;
                    if (MessageBox.Show("黑棋赢\n是否再来一局？", "结果", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        reset();
                        label1.Text = "当前回合：黑棋";                                                
                        label4.Text = "用时：0:0:0";
                        timer1.Start();
                        return "BlackYes";
                    }
                    else
                    {
                        MessageBox.Show("黑棋获胜次数： " + black_win_times + "\n" + "白棋获胜次数：" + white_win_times);
                        pvp = false;
                        pve = false;
                        reset();
                        set_visible(false);
                        return "No";
                    }
                }                                                                                                  
                if (checkerboard[i, j] == -1)
                {
                    white_win_times++;
                    label2.Text = "黑棋获胜次数：" + black_win_times;
                    label3.Text = "白棋获胜次数：" + white_win_times;
                    if (MessageBox.Show("白棋赢\n是否再来一局？", "结果", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        reset();
                        label1.Text = "当前回合：黑棋";                                               
                        label4.Text = "用时：0:0:0";
                        timer1.Start();
                        return "WhiteYes";
                    }
                    else 
                    {
                        MessageBox.Show("黑棋获胜次数： " + black_win_times + "\n" + "白棋获胜次数：" + white_win_times);
                        pvp = false;
                        pve = false;
                        reset();
                        set_visible(false);
                        return "No";                       
                    }
                }
            }
            return null;            
        }

        //清空并重置棋盘
        void reset()
        {            
            for(int i = 0; i < 19; i++)
            {
                for(int j = 0; j < 19; j++) { checkerboard[i, j] = 0; }                                                                      
            }
            checker_count = 0;
            time = 0;            
            Invalidate();
        }

        //判断是否一局结束
        bool result()
        {
            for(int i = 0; i <= grid_number; i++)
            {
                for(int j = 0; j <= grid_number; j++)
                {                    
                    if(is_five(i, j, -1, -1) != null) { return true; }                                    
                    if(is_five(i, j, 0, -1) != null) { return true; }                    
                    if(is_five(i, j, 1, -1) != null) { return true; }                   
                    if(is_five(i, j, -1, 0) != null) { return true; }                    
                    if(is_five(i, j, 1, 0) != null) { return true; }                    
                    if(is_five(i, j, -1, 1) != null) { return true; }                    
                    if(is_five(i, j, 0, 1) != null) { return true; }                    
                    if(is_five(i, j, 1, 1) != null) { return true; }                   
                }
            }
            return false;
        }

        //控制电脑画棋子
        void computer_paint(SolidBrush b)
        {
            Graphics g = CreateGraphics();
            int max = -1;
            int number = 0;
            for (int i = 0; i <= grid_number; i++)
            {
                for (int j = 0; j <= grid_number; j++)
                {
                    weight[i, j] = 0;
                    if (checkerboard[i, j] == 0)
                    {
                        set_weight(i, j, -1);
                        if (weight[i, j] > max)
                        {
                            number = 1;
                            max = weight[i, j];
                            target[number, 0] = i;
                            target[number, 1] = j;
                        }
                        if (weight[i, j] == max)
                        {
                            number++;
                            target[number, 0] = i;
                            target[number, 1] = j;
                        }
                    }
                }
            }            
            n = random.Next(1, number + 1);
            checkerboard[target[n, 0], target[n, 1]] = -1;            
            g.FillEllipse(b, target[n, 0] * grid_size + 100 - grid_size / 3, target[n, 1] * grid_size + 50 - grid_size / 3, 2 * grid_size / 3, 2 * grid_size / 3);
            g.DrawEllipse(pen, target[n, 0] * grid_size + 100 - grid_size / 3, target[n, 1] * grid_size + 50 - grid_size / 3, 2 * grid_size / 3, 2 * grid_size / 3);
            //show_weight();
        }

        //绘制图面
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (pvp||pve)
            {
                Graphics g = e.Graphics;
                g.FillRectangle(board, 100, 50, grid_number * grid_size, grid_number * grid_size);
                for (int i = 0; i <= grid_number; i++)
                {
                    g.DrawLine(pen, 100, 50 + grid_size * i, 100 + grid_number * grid_size, 50 + grid_size * i);
                    g.DrawLine(pen, 100 + grid_size * i, 50, 100 + grid_size * i, 50 + grid_number * grid_size);
                }
                for (int i = 0; i < 19; i++)
                {
                    for (int j = 0; j < 19; j++)
                    {
                        if (checkerboard[i, j] == 1)
                        {
                            g.FillEllipse(black, i * grid_size + 100 - grid_size / 3, j * grid_size + 50 - grid_size / 3, 2 * grid_size / 3, 2 * grid_size / 3);
                            result();
                        }
                        if (checkerboard[i, j] == -1)
                        {                            
                            g.DrawEllipse(pen, i * grid_size + 100 - grid_size / 3, j * grid_size + 50 - grid_size / 3, 2 * grid_size / 3, 2 * grid_size / 3);
                            g.FillEllipse(white, i * grid_size + 100 - grid_size / 3, j * grid_size + 50 - grid_size / 3, 2 * grid_size / 3, 2 * grid_size / 3);
                            result();
                        }
                    }
                }
            }
        }

        //选棋盘底色
        private void 棋盘底色ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            board.Color = colorDialog1.Color;
            clear.Color = board.Color;
            Invalidate();
        }

        //选棋盘颜色
        private void 棋盘线颜色ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            pen.Color = colorDialog1.Color;
            Invalidate();
        }        

        //调用窗体时
        private void Form1_Load(object sender, EventArgs e)
        {
            set_button_position();
            set_lable_position();
        }

        //窗体大小改变时
        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            set_button_position();
            set_lable_position();
        }

        //点击双人对战
        private void button1_Click(object sender, EventArgs e)
        {
            pvp = true;
            set_label();
        }

        //计量本局所用时间并显示
        private void timer1_Tick(object sender, EventArgs e)
        {
            time++;
            int hour = time / 3600;
            int minute = (time - hour * 3600) / 60;
            int second = time - hour * 3600 - minute * 60;
            label4.Text = "用时：" + hour + ":" + minute + ":" + second;
        }

        //使格子大小变为35
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            grid_size = 35;
            set_lable_position();
            Invalidate();
        }

        //使格子大小变为30
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            grid_size = 30;
            set_lable_position();
            Invalidate();
        }

        //使格子大小变为25
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            grid_size = 25;
            set_lable_position();
            Invalidate();
        }

        //使格子大小变为20
        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            grid_size = 20;
            set_lable_position();
            Invalidate();
        }

        //点击双人对战按钮
        private void button3_Click(object sender, EventArgs e)
        {
            pve = true;            
            is_first = true;             
            set_label();
        }

        //设置棋盘格数量为16
        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("是否将棋盘格数目改为16？", "设置", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                grid_number = 16;
                black_win_times = 0;
                white_win_times = 0;
                label2.Text = "黑棋获胜次数：0";
                label3.Text = "白棋获胜次数：0";
                reset();
            }
        }

        //设置棋盘格数量为13
        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("是否将棋盘格数目改为13？", "设置", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                grid_number = 13;
                black_win_times = 0;
                white_win_times = 0;
                label2.Text = "黑棋获胜次数：0";
                label3.Text = "白棋获胜次数：0";
                reset();
            }
        }

        //设置棋盘格数量为10
        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("是否将棋盘格数目改为10？", "设置", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                grid_number = 10;
                black_win_times = 0;
                white_win_times = 0;
                label2.Text = "黑棋获胜次数：0";
                label3.Text = "白棋获胜次数：0";
                reset();
            }
        }

        //设置棋盘格数量为7
        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("是否将棋盘格数目改为7？", "设置", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                grid_number = 7;
                black_win_times = 0;
                white_win_times = 0;
                label2.Text = "黑棋获胜次数：0";
                label3.Text = "白棋获胜次数：0";
                reset();
            }
        }       

        //点击悔棋按钮
        private void button2_Click(object sender, EventArgs e)
        {
            Graphics g = CreateGraphics();
            if (pvp)
            {
                checkerboard[X, Y] = 0;
                checker_count--;                
                g.DrawEllipse(clear, X * grid_size + 100 - grid_size / 3, Y * grid_size + 50 - grid_size / 3, 2 * grid_size / 3, 2 * grid_size / 3);
                g.FillEllipse(board, X * grid_size + 100 - grid_size / 3, Y * grid_size + 50 - grid_size / 3, 2 * grid_size / 3, 2 * grid_size / 3);
                g.DrawLine(pen, X * grid_size + 100, Y * grid_size + 50 - grid_size / 3, X * grid_size + 100, Y * grid_size + 50 + grid_size / 3);
                g.DrawLine(pen, X * grid_size + 100 - grid_size / 3, Y * grid_size + 50, X * grid_size + 100 + grid_size / 3, Y * grid_size + 50);
                button2.Enabled = false;
            }
            if (pve)
            {
                checkerboard[target[n, 0], target[n, 1]] = 0;
                int X1 = target[n, 0];int Y1 = target[n, 1];
                if (X >= 0 && X <= grid_number && Y >= 0 && Y <= grid_number) { checkerboard[X, Y] = 0; }                
                g.DrawEllipse(clear, X * grid_size + 100 - grid_size / 3, Y * grid_size + 50 - grid_size / 3, 2 * grid_size / 3, 2 * grid_size / 3);
                g.FillEllipse(board, X * grid_size + 100 - grid_size / 3, Y * grid_size + 50 - grid_size / 3, 2 * grid_size / 3, 2 * grid_size / 3);
                g.DrawLine(pen, X * grid_size + 100, Y * grid_size + 50 - grid_size / 3, X * grid_size + 100, Y * grid_size + 50 + grid_size / 3);
                g.DrawLine(pen, X * grid_size + 100 - grid_size / 3, Y * grid_size + 50, X * grid_size + 100 + grid_size / 3, Y * grid_size + 50);
                g.DrawEllipse(clear, X1 * grid_size + 100 - grid_size / 3, Y1 * grid_size + 50 - grid_size / 3, 2 * grid_size / 3, 2 * grid_size / 3);
                g.FillEllipse(board, X1 * grid_size + 100 - grid_size / 3, Y1 * grid_size + 50 - grid_size / 3, 2 * grid_size / 3, 2 * grid_size / 3);
                g.DrawLine(pen, X1 * grid_size + 100, Y1 * grid_size + 50 - grid_size / 3, X1 * grid_size + 100, Y1 * grid_size + 50 + grid_size / 3);
                g.DrawLine(pen, X1* grid_size + 100 - grid_size / 3, Y1 * grid_size + 50, X1 * grid_size + 100 + grid_size / 3, Y1 * grid_size + 50);                
                button2.Enabled = false;
            }
        }

        //鼠标点击
        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            Graphics g = CreateGraphics();
            //如果是双人对战
            if (pvp)
            {
                if (e.Button == MouseButtons.Left)
                {
                    //获取画棋子的位置
                    X = checker_position(e.X, 100);
                    Y = checker_position(e.Y, 50);                    
                    if (X >= 0 && Y >= 0)
                    {
                        if (checkerboard[X, Y] == 0)
                        {
                            //画黑棋
                            if (checker_count % 2 == 0)
                            {                                
                                checkerboard[X, Y] = 1;
                                g.FillEllipse(black, X * grid_size + 100 - grid_size / 3, Y * grid_size + 50 - grid_size / 3, 2 * grid_size / 3, 2 * grid_size / 3);
                                label1.Text = "当前回合：白棋";
                                checker_count++;
                                result();
                                button2.Enabled = true;
                            }
                            //画白棋
                            else
                            {
                                if (checker_count % 2 == 1)
                                {
                                    checkerboard[X, Y] = -1;
                                    g.DrawEllipse(pen, X * grid_size + 100 - grid_size / 3, Y * grid_size + 50 - grid_size / 3, 2 * grid_size / 3, 2 * grid_size / 3);
                                    g.FillEllipse(white, X * grid_size + 100 - grid_size / 3, Y * grid_size + 50 - grid_size / 3, 2 * grid_size / 3, 2 * grid_size / 3);
                                    label1.Text = "当前回合：黑棋";
                                    checker_count++;
                                    result();
                                    button2.Enabled = true;
                                }
                            }
                        }
                    }
                }
            }
            //如果是人机对战
            if (pve)
            {
                //默认先手
                if (is_first)
                {                    
                    if (e.Button == MouseButtons.Left)
                    {
                        //获取鼠标点击的位置
                        X = checker_position(e.X, 100);
                        Y = checker_position(e.Y, 50);
                        if (X >= 0 && Y >= 0)
                        {
                            if (checkerboard[X, Y] == 0)
                            {
                                //画黑棋
                                checkerboard[X, Y] = 1;
                                g.FillEllipse(black, X * grid_size + 100 - grid_size / 3, Y * grid_size + 50 - grid_size / 3, 2 * grid_size / 3, 2 * grid_size / 3);
                                label1.Text = "当前回合：白棋";
                                button2.Enabled = true;
                                if (result() == false)
                                {
                                    //画白棋
                                    computer_paint(white);
                                    label1.Text = "当前回合：黑棋";
                                    result();
                                }
                            }
                        }
                    }
                }
            }
        }  
    }    
}
