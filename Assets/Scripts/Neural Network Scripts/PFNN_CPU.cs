using System.Collections;
using System.Collections.Generic;
using System.IO;

/*  Network inputs (total 342 elems):
 * 
 *  (12) total 48
 *    0 -  11 = trajectory position X coordinate
 *   12 -  23 = trajectory position Z coordinate
 *   24 -  35 = trajectory direction X coordinate
 *   36 -  47 = trajectory direction Z coordinate
 * 
 *  (12) total 72
 *   48 -  59 = trajectory gait stand
 *   60 -  71 = trajectory gait walk
 *   72 -  83 = trajectory gait jog
 *   84 -  95 = trajectory gait crouch
 *   96 - 107 = trajectory gait jump
 *  108 - 119 = unused, always 0.0. Reason why there isn't 330 elems as in paper.
 *  
 *  (31) total 186
 *  120 - 212 = joint positions (x,y,z). Every axis is on every third place.
 *  213 - 305 = joint velocities (x,y,z). Every axis is on every third place.
 *  
 *  (12) total 36
 *  306 - 317 = trajectory height, right point
 *  318 - 329 = trajectory height, middle point
 *  330 - 341 = trajectory height, left point
 *  
 *  ----------------------------------
 *  Network outputs (total 311 elems):
 *  
 *  0 = ? trajectory position, x axis ? (1950)
 *  1 = ? trajectory position, z axis ? (1950)
 *  2 = ? trajectory direction ?        (1952)
 *  3 = change in phase
 *  4 - 7 = ? something about IK weights ? (1730)
 *  
 *  (6) total 24
 *    8 -  13 = trajectory position, x axis
 *   14 -  19 = trajectory position, z axis
 *   20 -  25 = trajectory direction, x axis
 *   26 -  31 = trajectory direction, z axis
 *  
 *  (31) total 279
 *   32 - 124 = joint positions (x,y,z). Every axis is on every third place.
 *  125 - 217 = joint velocities (x,y,z). Every axis is on every third place.
 *  218 - 310 = joint rotations (x,y,z). Every axis is on every third place.
 */

public class PFNN_CPU {

    private const float PI = 3.14159274f;
    private string WeightsFolderPath = "D:\\Network weights\\";
    
    private int InputSize;
    private int OutputSize;
    private int NumberOfNeurons;

    public Matrix X, Y;                         // inputs, outputs
    private Matrix H0, H1;                      // hidden layers

    private Matrix Xmean, Xstd, Ymean, Ystd;
    private Matrix[] W0, W1, W2;                // weights
    private Matrix[] B0, B1, B2;                // biases

    public enum Mode {
        constant,
        linear,
        cubic
    }
    private Mode WeightsMode;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="weightsType">
    /// Determine number of location along the phase space. 
    /// 0 = Constant method, 50 locations. 
    /// 1 = Linear interpolation, 10 locations. 
    /// 2 = Cubic Catmull-Rom spline, 4 locations.
    /// </param>
    /// <param name="inputSize"></param>
    /// <param name="outputSize"></param>
    /// <param name="numberOfNeurons"></param>
    public PFNN_CPU (
        Mode weightsType = Mode.constant,
        int inputSize = 342, 
        int outputSize = 311, 
        int numberOfNeurons = 512
        ) {

        WeightsMode = weightsType;
        InputSize = inputSize;
        OutputSize = outputSize;
        NumberOfNeurons = numberOfNeurons;
        
        SetWeightsCount();
        SetLayerSize();

        LoadMeansAndStds();
        LoadWeights();
    }

    private void SetWeightsCount() {

        switch (WeightsMode) {
            case Mode.constant:
                W0 = new Matrix[50]; W1 = new Matrix[50]; W2 = new Matrix[50];
                B0 = new Matrix[50]; B1 = new Matrix[50]; B2 = new Matrix[50];
                break;
            case Mode.linear:
                W0 = new Matrix[10]; W1 = new Matrix[10]; W2 = new Matrix[10];
                B0 = new Matrix[10]; B1 = new Matrix[10]; B2 = new Matrix[10];
                break;
            case Mode.cubic:
                W0 = new Matrix[4]; W1 = new Matrix[4]; W2 = new Matrix[4];
                B0 = new Matrix[4]; B1 = new Matrix[4]; B2 = new Matrix[4];
                break;
        }
    }

    public void LoadMeansAndStds() {

        ReadDataFromFile(out Xmean, InputSize, "Xmean.bin");
        ReadDataFromFile(out Xstd, InputSize, "Xstd.bin");
        ReadDataFromFile(out Ymean, OutputSize, "Ymean.bin");
        ReadDataFromFile(out Ystd, OutputSize, "Ystd.bin");
    }

    public void LoadWeights() {        

        int j;
        switch (WeightsMode) {            
            case Mode.constant:
                for (int i = 0; i < 50; i++) {
                    
                    ReadDataFromFile(out W0[i], NumberOfNeurons, InputSize, string.Format("W0_{0:000}.bin", i));
                    ReadDataFromFile(out W1[i], NumberOfNeurons, NumberOfNeurons, string.Format("W1_{0:000}.bin", i));
                    ReadDataFromFile(out W2[i], OutputSize, NumberOfNeurons, string.Format("W2_{0:000}.bin", i));

                    ReadDataFromFile(out B0[i], NumberOfNeurons, string.Format("b0_{0:000}.bin", i));
                    ReadDataFromFile(out B1[i], NumberOfNeurons, string.Format("b1_{0:000}.bin", i));
                    ReadDataFromFile(out B2[i], OutputSize, string.Format("b2_{0:000}.bin", i));
                }
                break;
            case Mode.linear:                
                for (int i = 0; i < 10; i++) {
                    j = i * 5;

                    ReadDataFromFile(out W0[i], NumberOfNeurons, InputSize, string.Format("W0_{0:000}.bin", j));
                    ReadDataFromFile(out W1[i], NumberOfNeurons, NumberOfNeurons, string.Format("W1_{0:000}.bin", j));
                    ReadDataFromFile(out W2[i], OutputSize, NumberOfNeurons, string.Format("W2_{0:000}.bin", j));

                    ReadDataFromFile(out B0[i], NumberOfNeurons, string.Format("b0_{0:000}.bin", j));
                    ReadDataFromFile(out B1[i], NumberOfNeurons, string.Format("b1_{0:000}.bin", j));
                    ReadDataFromFile(out B2[i], OutputSize, string.Format("b2_{0:000}.bin", j));
                }
                break;
            case Mode.cubic:
                for (int i = 0; i < 4; i++) {
                    j = (int)(i * 12.5);

                    ReadDataFromFile(out W0[i], NumberOfNeurons, InputSize, string.Format("W0_{0:000}.bin", j));
                    ReadDataFromFile(out W1[i], NumberOfNeurons, NumberOfNeurons, string.Format("W1_{0:000}.bin", j));
                    ReadDataFromFile(out W2[i], OutputSize, NumberOfNeurons, string.Format("W2_{0:000}.bin", j));

                    ReadDataFromFile(out B0[i], NumberOfNeurons, string.Format("b0_{0:000}.bin", j));
                    ReadDataFromFile(out B1[i], NumberOfNeurons, string.Format("b1_{0:000}.bin", j));
                    ReadDataFromFile(out B2[i], OutputSize, string.Format("b2_{0:000}.bin", j));
                }
                break;
        }
    }

    private void ReadDataFromFile(out Matrix item, int rows, string fileName) {

        item = new Matrix(rows);

        string fullPath = WeightsFolderPath + fileName;
        float value;
        if (File.Exists(fullPath)) {
            using (BinaryReader reader = new BinaryReader(File.Open(fullPath, FileMode.Open))) {

                for (int i = 0; i < rows; i++) {
                    value = reader.ReadSingle();
                    item[i] = value;
                }
            }
        }
    }

    private void ReadDataFromFile(out Matrix item, int rows, int columns, string fileName) {

        item = new Matrix(rows, columns);

        string fullPath = WeightsFolderPath + fileName;
        float value;
        if (File.Exists(fullPath)) {
            using (BinaryReader reader = new BinaryReader(File.Open(fullPath, FileMode.Open))) {

                for (int i = 0; i < rows; i++) {
                    for (int j = 0; j < columns; j++) {
                        value = reader.ReadSingle();
                        item[i, j] = value;
                    }
                }
            }
        }
    }

    private void SetLayerSize() {

        X = new Matrix(InputSize);
        Y = new Matrix(OutputSize);

        H0 = new Matrix(NumberOfNeurons);
        H1 = new Matrix(NumberOfNeurons);
    }

    /// <summary>
    /// Main function for computing Neural Network result.
    /// </summary>
    /// <param name="p">Phase value.</param>
    public void Compute(float p) {

        int pIndex0;

        X = (X - Xmean) / Xstd;

        switch (WeightsMode) {
            case Mode.constant:
                pIndex0 = (int)((p / (2 * PI)) * 50);

                // Layer 1
                H0 = (W0[pIndex0] * X) + B0[pIndex0];
                H0.ELU();

                // Layer 2
                H1 = (W1[pIndex0] * H0) + B1[pIndex0];
                H1.ELU();

                // Layer 3, network output
                Y = (W2[pIndex0] * H1) + B2[pIndex0];
                break;

            case Mode.linear:
                break;

            case Mode.cubic:
                break;
        }

        Y = (Y * Ystd) + Ymean;
    }

    public void Reset() {

        Y = Ymean;
    }


}
