using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static KVerticesMinimumSpanningTree.Graph;

namespace KVerticesMinimumSpanningTree
{
    public class Graph
    {
        public int V, E;

        public Edge[] edgeList;

        public Verticle[] verticleList;

        public Graph(int v, int e)
        {
            V = v;
            E = e;
            verticleList = new Verticle[V];
            edgeList = new Edge[E];
            for (int i = 0; i < e; ++i)
                edgeList[i] = new Edge();
        }

        public class Verticle
        {
            public int x_coord, y_coord;
        }

        public class Edge : IComparable<Edge>
        {
            public int src, dest, weight;

            public int CompareTo(Edge compareEdge)
            {
                return weight - compareEdge.weight;
            }
        }

        // Класс для представления подмножества для объединения-поиска
        public class Subset
        {
            public int parent, rank;
        }

        // Поиск набора элементов i (использует метод сжатия пути)
        public int Find(Subset[] subsets, int i)
        {
            // Найти корень и сделать корень родителем i (сжатие пути)
            if (subsets[i].parent != i)
                subsets[i].parent
                    = Find(subsets, subsets[i].parent);

            return subsets[i].parent;
        }

        // Объединение двух наборов x и y (использует объединение по рангу)
        public void Union(Subset[] subsets, int x, int y)
        {
            int xroot = Find(subsets, x);
            int yroot = Find(subsets, y);

            // Прикрепляем дерево меньшего ранга под корнем дерева высокого ранга (объединенияем по рангу)
            if (subsets[xroot].rank < subsets[yroot].rank)
                subsets[xroot].parent = yroot;
            else if (subsets[xroot].rank > subsets[yroot].rank)
                subsets[yroot].parent = xroot;

            // Если ранги одинаковы, то делаем его корневым и увеличиваем его ранг на единицу
            else
            {
                subsets[yroot].parent = xroot;
                subsets[xroot].rank++;
            }
        }

        public void ReadFile(string fileName)
        {
            using var reader = new StreamReader(fileName);
            string line;
            reader.ReadLine();
            //V = Convert.ToInt32(Regex.Match(line, @"\d+").Value);
            // E = (V * (V - 1)) / 2;
            int i = 0;
            while ((line = reader.ReadLine()) != null)
            {
                string[] tokens = line.Split();
                verticleList[i] = new Verticle
                {
                    x_coord = int.Parse(tokens[0]),
                    y_coord = int.Parse(tokens[1])
                };
                i++;
            }
        }

        public static int CalcVertDistance(Verticle vert1, Verticle vert2)
        {
            return Math.Abs(vert1.x_coord - vert2.x_coord) + Math.Abs(vert1.y_coord - vert2.y_coord);
        }

        // Главный метод
        public void KruskalMST()
        {
            List<Edge> myList = new List<Edge>();
            int resultIndex = 0;
            int i;

            Array.Sort(edgeList);// Сортируем по возрастанию ребра

            // Создание подмножеств ребер
            Subset[] subsets = new Subset[V];
            for (int v = 0; v < V; ++v)
            {
                subsets[v] = new Subset
                {
                    parent = v,
                    rank = 0
                };
            }

            i = 0; // Для прохода по основному массиву с ребрами
            while (resultIndex < V - 1)
            {
                // Берем наименьшее ребро + УВЕЛИЧИВАЕМ индекс для следующей итерации
                Edge next_edge = edgeList[i++];

                int x = Find(subsets, next_edge.src);
                int y = Find(subsets, next_edge.dest);

                // Если добавление этого ребра не создает цикл, то добавляем его в результат и увеличьте индекс результата для следующего ребра.
                if (x != y)
                {
                    myList.Add(next_edge);
                    resultIndex++;
                    Union(subsets, x, y);
                }
            }

            // ТУТ ГРЯЗЬ
            for (int vertIndx = 0; vertIndx < V; vertIndx++)
            {
                Edge[] edgeListTemp = new Edge[V - 1]; // Список ребер, в которых учавствует данная вершина
                int edgeIndx = 0;
                foreach (Edge edge in this.edgeList)
                    if (edge.src == vertIndx || edge.dest == vertIndx)
                        edgeListTemp[edgeIndx++] = edge;

                Array.Sort(edgeListTemp); // Сортируем по возрастанию ребра
                foreach (Edge edgeTemp in edgeListTemp)
                {
                    if ((!isVertHas4Edge(myList, edgeTemp.src)) && (!isVertHas4Edge(myList, edgeTemp.dest)))
                    {
                        if (!myList.Contains(edgeTemp))
                            myList.Add(edgeTemp);
                    }
                    //else break;
                }
            }



            Dictionary<int, int> verts = new Dictionary<int, int>();
            foreach (Edge edgeTemp in myList)
            {
                if (verts.ContainsKey(edgeTemp.src))
                {
                    verts[edgeTemp.src]++;
                }
                else
                {
                    verts.Add(edgeTemp.src, 1);
                }
                if (verts.ContainsKey(edgeTemp.dest))
                {
                    verts[edgeTemp.dest]++;
                }
                else
                {
                    verts.Add(edgeTemp.dest, 1);
                }
            }

            string str2 = "";
            foreach (var vla in verts)
            {
                Console.WriteLine(vla.Key + ":" + vla.Value);
                if (!(vla.Value == 4))
                {
                    str2 = str2 + "__" + "(" + vla.Key + ":" + vla.Value + ")";
                }
            }
            Console.WriteLine(str2);
            //---------------------------------------------------Результат---------------------------------------------------
            int treeWeight = 0;
            int maxEdgeWeight = 0;
            StreamWriter sw = new StreamWriter("D:\\Users\\Андрей\\Desktop\\GaraninAndrey_" + V + "_1.txt", true, Encoding.UTF8);
            foreach (Edge edgeTemp in myList)
            {
                sw.WriteLine("e" + " " + ++edgeTemp.src + " " + ++edgeTemp.dest);
                treeWeight += edgeTemp.weight;
                if (edgeTemp.weight > maxEdgeWeight)
                    maxEdgeWeight = edgeTemp.weight;
            }
            sw.WriteLine("c " + "Вес 4-регулярного подграфа = " + treeWeight + ", самое длинное ребро = " + maxEdgeWeight);
            sw.WriteLine("p edge " + V + " " + myList.Count);
            Console.WriteLine("Done!");
            sw.Close();
            Console.ReadLine();
        }

        bool isVertHas4Edge(List<Edge> result, int vertIdx)
        {
            int count = 0;
            foreach (Edge edge in result) // Проходим по всем ребрам, которые содержат текущую вершину
            {
                if (edge.src == vertIdx || edge.dest == vertIdx)
                {
                    count++;
                    if (count == 4)
                        return true;
                }
            }
            return false;
        }
    }

    class Program
    {
        public static void Main(String[] args)
        {
            //int V = 4; int E = 6;
            int V = 64; int E = 2016;
            //int V = 128;  int E = 8128;
            //int V = 512;  int E = 130816;
            //int V = 2048; int E = 2096128;
            //int V = 4096; int E = 8386560;
            Graph graph = new Graph(V, E);
            graph.ReadFile(@"data\\Taxicab_64.txt");
            int v2Index = 0;
            int edgeIndex = 0;
            for (int v1Index = 0; v1Index < V - 1; v1Index++)
            {
                v2Index++;
                Verticle vert1 = new Verticle
                {
                    x_coord = graph.verticleList[v1Index].x_coord,
                    y_coord = graph.verticleList[v1Index].y_coord
                };
                for (int i = v2Index; i < V; i++)
                {
                    Verticle vert2 = new Verticle
                    {
                        x_coord = graph.verticleList[i].x_coord,
                        y_coord = graph.verticleList[i].y_coord
                    };

                    int distance = CalcVertDistance(vert1, vert2);

                    graph.edgeList[edgeIndex] = new Edge
                    {
                        src = v1Index,
                        dest = i,
                        weight = distance
                    };
                    edgeIndex++;
                }
            }
            graph.KruskalMST();
        }
    }
}