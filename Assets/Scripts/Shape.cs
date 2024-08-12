
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public enum RotateAngle {
    Zero, Clockwise90, Clockwise180, CounterClockwise90
}

public class Shape
{
    public int[,] matrix;
    public int Row => matrix.GetLength(0);
    public int Col => matrix.GetLength(1);

    public Shape(int row, int col) {
        matrix = new int[row, col];
    }

    public Shape(int[,] mat) {
        matrix = mat;
    }

    public Shape Clone() {
        return (Shape)MemberwiseClone();
    }

    public void Rotate(RotateAngle angle) {
        matrix = Rotate(in matrix, angle);
    }

    public static Shape Rotate(Shape shape, RotateAngle angle) {
        var mat = Rotate(in shape.matrix, angle);
        return new Shape(mat);
    }

    public static int[,] Rotate(in int[,] mat, RotateAngle angle) {
        int rows = mat.GetLength(0);
        int cols = mat.GetLength(1);
        int[,] rotatedMatrix;
        if (angle == RotateAngle.Clockwise90) {
            rotatedMatrix = new int[cols, rows];
    
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    rotatedMatrix[j, rows - 1 - i] = mat[i, j];
                }
            }
            return rotatedMatrix;
        } else if (angle == RotateAngle.Clockwise180) {
            rotatedMatrix = new int[rows, cols];
    
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    rotatedMatrix[rows - 1 - i, cols - 1 - j] = mat[i, j];
                }
            }
            return rotatedMatrix;
        } else if (angle == RotateAngle.CounterClockwise90) {
            rotatedMatrix = new int[cols, rows];
    
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    rotatedMatrix[cols - 1 - j, i] = mat[i, j];
                }
            }
            return rotatedMatrix;
        }
        
        rotatedMatrix = mat;
        return rotatedMatrix;
    }

    public void Filp(int axis = 0) {
        matrix = Filp(in matrix, axis);
    }

    public static int[,] Filp(in int[,] mat, int axis = 0) {
        int rows = mat.GetLength(0);
        int cols = mat.GetLength(1);
        int[,] flipedMatrix = new int[rows, cols];
        if (axis == 0) {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    flipedMatrix[i, cols - 1 - j] = mat[i, j];
                }
            }
        } else if (axis == 1) {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    flipedMatrix[rows - 1 - i, j] = mat[i, j];
                }
            }
        } else {
            flipedMatrix = mat;
        }
        return flipedMatrix;
    }

    public int Index2DTo1D(int row, int col) {
        return Row * row + col;
    }

    public void Index1DTo2D(int idx, out int row, out int col) {
        row = idx / Col;
        col = idx % Col;
    }
}

public class ShapePrefabs
{
    public static readonly Shape[] shapes = {
        new(new int[,] {{1}}),
        new(new int[,] {{1, 1}}),
        new(new int[,] {{1, 1, 1}}),
        new(new int[,] {{1, 1},
                        {1, 0}}),
        new(new int[,] {{1, 1, 1, 1}}),
        new(new int[,] {{1, 1},
                        {1, 1}}),
        new(new int[,] {{1, 1, 1},
                        {1, 0, 0}}),
        new(new int[,] {{1, 1, 1},
                        {0, 1, 0}}),
        new(new int[,] {{1, 1, 0},
                        {0, 1, 1}})
    };

    public static readonly int[] numbers = {1, 2, 3, 3, 4, 4, 4, 4, 4};

    public static readonly int Length = 9;

    public static ShapeInt GetShapeInt(int shapeIdx) {
        return new ShapeInt(shapes[shapeIdx]);
    }
}


public class ShapeInt
{
    private static readonly float[] RotationMatrix = new float[] { 0, 1, -1, 0 };
    // private static readonly float[] rRotationMatrix = new float[] { 0, -1, 1, 0 };

    public List<Vector2Int> matrix;
    public List<int> values;
    public int MinX = 9999;
    public int MinY = 9999;
    public int MaxX = -9999;
    public int MaxY = -9999;
    public int Height { get => MaxY - MinY + 1; }
    public int Width { get => MaxX - MinX + 1; }
    public int Size { get => Math.Max(MaxX - MinX, MaxY - MinY) + 1; }
    public int CenterPointIndex = -1;

    public ShapeInt(List<Vector2Int> mat) {
        matrix = mat;
        values = Enumerable.Repeat(0, matrix.Count).ToList();
        NormalizeMatrix();
        GetRowCol();
    }

    public ShapeInt(List<Vector2Int> mat, List<int> val) {
        matrix = mat;
        values = val;
        NormalizeMatrix();
        GetRowCol();
    }

    public ShapeInt(int[,] mat) {
        matrix = new List<Vector2Int>();
        values = new List<int>();
        for (int i = 0; i < mat.GetLength(0); i++) {
            for (int j = 0; j < mat.GetLength(1); j++) {
                if (mat[i, j] > 0){
                    matrix.Add(new(j, -i));
                    values.Add(mat[i, j]);
                }
            }
        }
        NormalizeMatrix();
        GetRowCol();
    }

    public ShapeInt(Shape shape) {
        List<Vector2Int> mat = new();
        for (int i = 0; i < shape.Row; i++) {
            for (int j = 0; j < shape.Col; j++) {
                if (shape.matrix[i, j] > 0)
                    mat.Add(new(j, -i));
            }
        }
        matrix = mat;
        values = Enumerable.Repeat(0, matrix.Count).ToList();
        NormalizeMatrix();
        GetRowCol();
    }

    public ShapeInt Clone() {
        List<Vector2Int> newMat = new(matrix);
        List<int> newVal = new(values);
        return new(newMat, newVal);
    }

    override public string ToString() {
        string str = "";
        foreach(var vec in matrix) {
            str += vec + " ";
        }
        return str;
    }

    public bool Contains(Vector2Int point) {
        return matrix.Contains(point);
    }

    public void GetRowCol() {
        MinX = 9999;
        MinY = 9999;
        MaxX = -9999;
        MaxY = -9999;
        foreach (var point in matrix) {
            MinX = Math.Min(MinX, point.x);
            MinY = Math.Min(MinY, point.y);
            MaxX = Math.Max(MaxX, point.x);
            MaxY = Math.Max(MaxY, point.y);
        }
    }

    private void GetCenterPoint() {
        Vector2Int virtualCenter = new(0, 0);
        foreach (var point in matrix) {
            virtualCenter += point;
        }
        virtualCenter /= matrix.Count;
        float dist = 9999;
        for (int i = 0; i < matrix.Count; i++) {
            var point = matrix[i];
            float curDist = Math.Abs(point.x - virtualCenter.x) + Math.Abs(point.y - virtualCenter.y);
            if (curDist < dist) {
                dist = curDist;
                CenterPointIndex = i;
            }
        }
    }

    public void SortPoints() {
        matrix.Sort((x, y) => x.y.CompareTo(y.y));
    }

    private void NormalizeMatrix() {
        GetCenterPoint();
        Vector2Int centerPoint = new(matrix[CenterPointIndex].x, matrix[CenterPointIndex].y);
        for (int i = 0; i < matrix.Count; i++) {
            matrix[i] -= centerPoint;
        }
    }

    public void Rotate(RotateAngle angle) {
        int direction = 0;
        if (angle == RotateAngle.Clockwise90) direction = -1;
        if (angle == RotateAngle.CounterClockwise90) direction = 1;
        for (int i = 0; i < matrix.Count; i++)
        {
            Vector2 cell = (Vector2)matrix[i];

            int x, y;

            if (Size % 2 == 0) {
                cell.x -= 0.5f;
                cell.y -= 0.5f;
                x = Mathf.CeilToInt((cell.x * RotationMatrix[0] * direction) + (cell.y * RotationMatrix[1] * direction));
                y = Mathf.CeilToInt((cell.x * RotationMatrix[2] * direction) + (cell.y * RotationMatrix[3] * direction));
            } else {
                x = Mathf.RoundToInt((cell.x * RotationMatrix[0] * direction) + (cell.y * RotationMatrix[1] * direction));
                y = Mathf.RoundToInt((cell.x * RotationMatrix[2] * direction) + (cell.y * RotationMatrix[3] * direction));
            }

            matrix[i] = new Vector2Int(x, y);

            int val = values[i] % 16;
            if (direction == 1) {
                val = (val << 1) % 15;
            } else {
                val >>= 1;
                if (values[i] % 2 == 1)
                    val |= 8;
            }

            if (val == 0) val = 16;
            values[i] = val;
        }
        NormalizeMatrix();
        GetRowCol();
    }

    public void Filp(int axis = 0) {
        for (int i = 0; i < matrix.Count; i++) {
            int x = matrix[i].x;
            int y = matrix[i].y;
            if (axis == 0)
                matrix[i] = new Vector2Int(-x, y);
            else
                matrix[i] = new Vector2Int(x, -y);
        }
        GetRowCol();
    }
}