using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TakBrain
{
    /// <summary>
    /// Represents the board as a graph where vertices are connected if they are owned by the same player
    /// and the stacks are covered by either a flat or a capstone (i.e. walls don't count)
    /// 
    /// Uses adjacency lists to represent the graph
    /// </summary>
    public class BoardGraph
    {
        public BoardGraph(TakBoard board, TakPiece.PieceColor player)
        {
            Vertices = new List<Vertex>();
            VerticesByCoordinate = new Dictionary<Point, Vertex>();
            for(int i=0; i<board.Size; i++)
            {
                for (int j = 0; j < board.Size; j++)
                {
                    if (board[i, j].Owner == player && !board[i, j].Top.IsWall)
                    {
                        Vertex v = new Vertex() { Row = i, Column = j };

                        VerticesByCoordinate.Add(new Point(i, j), v);
                        Vertices.Add(v);
                    }
                }
            }

            for(int i=0; i<Vertices.Count; i++)
            {
                Vertex p = Vertices[i];
                for (int j = i; j < Vertices.Count; j++)
                {
                    Vertex q = Vertices[j];

                    if ((Math.Abs(q.Row - p.Row) == 1 && q.Column == p.Column) ||
                        (Math.Abs(q.Column - p.Column) == 1 && q.Row == p.Row))
                    {
                        q.Adjacencies.Add(p);
                        p.Adjacencies.Add(q);
                    }
                }
            }
        }

        /// <summary>
        /// Determines if two stacks located at the given coordinates are connected via a road
        /// </summary>
        /// <param name="startRow"></param>
        /// <param name="startCol"></param>
        /// <param name="endRow"></param>
        /// <param name="endCol"></param>
        /// <returns></returns>
        public bool IsConnected(int startRow, int startCol, int endRow, int endCol)
        {
            Point p = new Point(startRow, startCol);
            Point q = new Point(endRow, endCol);

            if(VerticesByCoordinate.Keys.Contains(p) && VerticesByCoordinate.Keys.Contains(q))
            {
                // both endpoints exist in the graph, so now we need to see if they are actually connected along a path

                Dictionary<Vertex, bool> visited = new Dictionary<Vertex, bool>();
                foreach (Vertex v in Vertices)
                    visited.Add(v, false);

                Vertex start = VerticesByCoordinate[p];
                Vertex end = VerticesByCoordinate[q];
                Vertex current = start;
                List<Vertex> queue = new List<Vertex>();
                foreach (Vertex v in start.Adjacencies)
                    queue.Add(v);
                bool done = false;
                visited[start] = true;
                while (!done)
                {
                    if (current == end || queue.Count == 0)
                        done = true;
                    else
                    {
                        // go to the next node in the queue
                        current = queue[0];
                        queue.RemoveAt(0);
                        visited[current] = true;

                        foreach(Vertex v in current.Adjacencies)
                        {
                            if (!visited[v])
                                queue.Add(v);
                        }
                    }
                }

                return current == end;
            }
            else
            {
                return false;
            }
        }

        private List<Vertex> Vertices;
        private Dictionary<Point, Vertex> VerticesByCoordinate;

        private class Vertex
        {
            public int Row, Column;

            public List<Vertex> Adjacencies = new List<Vertex>();
        }
    }
}
