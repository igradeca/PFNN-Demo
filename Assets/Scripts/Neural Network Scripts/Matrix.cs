using System;
using System.Collections;
using System.Collections.Generic;

public class Matrix {

    private float[] data;
    private int rows, columns;

    public Matrix(int rows, int columns) {

        this.rows = rows;
        this.columns = columns;
        data = new float[rows * columns];
    }

    /// <summary>
    /// Creates vertical one-dimensional matrix.
    /// </summary>
    /// <param name="rows"></param>
    public Matrix(int rows) : this(rows, 1) {}

    public Matrix(float[,] data) : this(data.GetLength(0), data.GetLength(1)) {

        int index = 0;
        for (int row = 0; row < rows; row++) {
            for (int column = 0; column < columns; column++) {
                this.data[index] = data[row, column];
                ++index;
            }
        }
    }

    public Matrix(Matrix mat) : this(mat.rows, mat.columns) {

        for (int i = 0; i < data.Length; i++) {
            data[i] = mat.data[i];
        }
    }

    public float this[int row, int column] {
        get {
            return data[(row * this.columns) + column];
        }
        set {
            data[(row * this.columns) + column] = value;
        }
    }

    /// <summary>
    /// Returns first element in selected row. It should be used for vertical one-dimensional matrices.
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    public float this[int row] {
        get {
            return data[(row * this.columns) + 0];
        }
        set {
            data[(row * this.columns) + 0] = value;
        }
    }
    
    public static Matrix operator + (Matrix mat1, Matrix mat2) {

        if (mat1.HasSameDimensions(mat2)) {
            Matrix result = new Matrix(mat1.rows, mat2.columns);

            for (int i = 0; i < mat1.data.Length; i++) {
                result.data[i] = mat1.data[i] + mat2.data[i];
            }
            return result;

        } else {
            throw new InvalidMatrixDimensionsException("Adding not possible. Matrix dimensions do not match.");
        }
    }

    public static Matrix operator - (Matrix mat1, Matrix mat2) {

        if (mat1.HasSameDimensions(mat2)) {
            Matrix result = new Matrix(mat1.rows, mat2.columns);

            for (int i = 0; i < mat1.data.Length; i++) {
                result.data[i] = mat1.data[i] - mat2.data[i];
            }
            return result;

        } else {
            throw new InvalidMatrixDimensionsException("Adding not possible. Matrix dimensions do not match.");
        }
    }

    public bool HasSameDimensions(Matrix mat) {

        if ((this.rows == mat.rows) && (this.columns == mat.columns)) {
            return true;
        } else {
            return false;
        }
    }

    public static Matrix operator * (Matrix mat1, Matrix mat2) {

        if (mat1.AreMatricesSameSizeAndVertical(mat2)) {                   // Fake matrix multiplying.
            Matrix result = new Matrix(mat1.rows, 1);

            for (int i = 0; i < mat1.rows; i++) {
                result[i, 0] = mat1[i, 0] * mat2[i, 0];
            }
            return result;

        } else if (mat1.IsMultiplicationPossible(mat2)) {
            Matrix result = new Matrix(mat1.rows, mat2.columns);

            for (int i = 0; i < mat1.rows; i++) {
                MultiplyRow(i, mat1, mat2, ref result);
            }
            return result;

        } else {
            throw new InvalidMatrixDimensionsException("Multiplying not possible. First matrix column size is not same as second matrix rows.");
        }
    }

    public bool AreMatricesSameSizeAndVertical(Matrix mat) {

        if (this.rows == mat.rows && this.columns == 1 && mat.columns == 1) {
            return true;
        } else {
            return false;
        }
    }

    public bool IsMultiplicationPossible(Matrix mat) {

        if (this.columns == mat.rows) {
            return true;
        } else {
            return false;
        }
    }

    public static void MultiplyRow(int row, Matrix mat1, Matrix mat2, ref Matrix resultMat) {

        int mat1Index = row * mat1.columns;
        int mat2Index;

        for (int column = 0; column < resultMat.columns; column++) {
            float result = 0;
            mat2Index = column;

            for (int i = 0; i < mat1.columns; i++) {
                result += mat1.data[mat1Index + i] * mat2.data[mat2Index];
                mat2Index += mat2.columns;
            }

            resultMat[row, column] = result;
        }
    }

    /* 
     * !!!  Use with caution  !!!
     * This is not 'real' matrix division simply because there is no defined process for dividing a matrix by another matrix.
     * This operation should be used only for vertical matrices of same size (Nx1) to divide each element of mat_1 with mat_2.
     */
    public static Matrix operator / (Matrix mat1, Matrix mat2) {

        if (mat1.IsMatrixVertical() && mat2.IsMatrixVertical()) {
            Matrix result = new Matrix(mat1.rows, 1);

            for (int i = 0; i < mat1.rows; i++) {
                result[i, 0] = mat1[i, 0] / mat2[i, 0];
            }
            return result;

        } else {
            throw new InvalidMatrixDimensionsException(
                "Both matrices must be vertical. " +
                "This operation should be used only for vertical matrices of same size (Nx1) to divide each element of mat_1 with mat_2."
                );
        }
    }

    private bool IsMatrixVertical() {

        if (this.columns == 1) {
            return true;
        } else {
            return false;
        }
    }

    /// <summary>
    /// Exponential Linear Unit (ELU), activation function mostly used in Neural Networks.
    /// </summary>
    public void ELU() {

        for (int i = 0; i < this.data.Length; i++) {
            data[i] = (float)(Math.Max(data[i], 0) + Math.Exp(Math.Min(data[i], 0)) - 1);
        }
    }

}

public class InvalidMatrixDimensionsException : InvalidOperationException {
    public InvalidMatrixDimensionsException() {}

    public InvalidMatrixDimensionsException(string message)
        : base(message) {
    }

    public InvalidMatrixDimensionsException(string message, Exception inner)
        : base(message, inner) {
    }
}
