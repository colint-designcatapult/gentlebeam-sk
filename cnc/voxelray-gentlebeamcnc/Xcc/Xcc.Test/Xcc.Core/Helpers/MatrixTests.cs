using Xcc.Core.Helpers;

namespace Xcc.Test.Xcc.Core.Helpers
{
    public class MatrixTests
    { 
        [SetUp]
        public void SetUp()
        {
            G.SetupCulture();
        }
        
        #region Constructor
        
        [Test]
        public void Constructor(
            [Random(0, 100, 3)] int rows,
            [Random(0, 100, 3)] int cols)
        {
            var m = new Matrix(rows, cols);
            
            // Check defaults
            Assert.That(m.mat, Is.Not.Null);
            Assert.That(m.rows, Is.EqualTo(rows));
            Assert.That(m.cols, Is.EqualTo(cols));
            Assert.That(m.L, Is.Null);
            Assert.That(m.U, Is.Null);
        }
        
        [Test]
        public void Constructor_ManualCases(
            [Values(0, 1, 9999)] int rows,
            [Values(0, 1, 9999)] int cols)
        {
            var m = new Matrix(rows, cols);
            
            // Check defaults
            Assert.That(m.mat, Is.Not.Null);
            Assert.That(m.rows, Is.EqualTo(rows));
            Assert.That(m.cols, Is.EqualTo(cols));
            Assert.That(m.L, Is.Null);
            Assert.That(m.U, Is.Null);
        }
        
        #endregion Constructor
        
        #region IsSquare
        
        [Test]
        public void IsSquare_True(
            [Random(0, 100, 3)] int size)
        {
            var m = new Matrix(size, size);
            Assert.That(m.IsSquare(), Is.True);
        }
        
        [Test]
        public void IsSquare_False(
            [Random(0, 100, 3)] int size,
            [Random(1, 100, 3)] int shift)
        {
            {
                var rows = size;
                var cols = rows + shift;
                var m = new Matrix(rows, cols);
                Assert.That(m.IsSquare(), Is.False);   
            }
            {
                var cols = size;
                var rows = cols + shift;
                var m = new Matrix(rows, cols);
                Assert.That(m.IsSquare(), Is.False);   
            }
        }
        
        #endregion IsSquare
        
        #region operator[row, col]
        
        [Test]
        public void Operator_Indexer()
        {
            var matrix = new Matrix(2, 2);
            matrix[0, 0] = 1.5;
            matrix[0, 1] = 2.5;
            matrix[1, 0] = 3.5;
            matrix[1, 1] = 4.5;
        
            Assert.That(matrix[0, 0], Is.EqualTo(1.5));
            Assert.That(matrix[0, 1], Is.EqualTo(2.5));
            Assert.That(matrix[1, 0], Is.EqualTo(3.5));
            Assert.That(matrix[1, 1], Is.EqualTo(4.5));
        }
        
        [Test]
        public void Operator_Indexer_ThrowsException(
            [Values(0, 1, 9999)] int rows,
            [Values(0, 1, 9999)] int cols)
        {
            var matrix = new Matrix(rows, cols);

            Assert.Throws<IndexOutOfRangeException>(() => { var a = matrix[-1, -1]; });
            Assert.Throws<IndexOutOfRangeException>(() => { var a = matrix[-1, 0]; });
            Assert.Throws<IndexOutOfRangeException>(() => { var a = matrix[0, -1]; });
            Assert.Throws<IndexOutOfRangeException>(() => { var a = matrix[rows, cols]; });
            Assert.Throws<IndexOutOfRangeException>(() => { var a = matrix[rows, 0]; });
            Assert.Throws<IndexOutOfRangeException>(() => { var a = matrix[0, cols]; });
        }
        
        #endregion operator[row, col]
        
        #region CreateMatrix(rows, cols, params double[] values)
        
        [Test]
        public void CreateMatrix(
            [Values(0, 1, 2, 3, 4)] int rows,
            [Values(0, 1, 2, 3, 4)] int cols)
        {
            var m = Matrix.Create(rows, cols);
            
            Assert.That(m.rows, Is.EqualTo(rows));
            Assert.That(m.cols, Is.EqualTo(cols));
            // Check all values are zero or empty
            Assert.That(m.GetElements(), Is.All.EqualTo(0.0).Within(G.Precision));
        }
        
        [Test]
        public void CreateMatrix_Throws_With_ManyParams(
            [Values(1, 2, 3, 4)] int rows,
            [Values(1, 2, 3, 4)] int cols)
        {
            var invalidElementCount = rows * cols + 1;
            var elements = Enumerable.Range(0, invalidElementCount).Select(x => (double)x).ToArray();
            Assert.Throws<IndexOutOfRangeException>(() => { var m = Matrix.Create(rows, cols, elements); });
        }
        
        [Test]
        public void CreateMatrix_Params_1x1()
        {
            var m = Matrix.Create(1, 1, 
                1.0);
            
            Assert.That(m[0, 0], Is.EqualTo(1.0).Within(G.Precision));
        }
        
        [Test]
        public void CreateMatrix_Params_1x2()
        {
            var m = Matrix.Create(1, 2, 
                1.0, 2.0);
            
            Assert.That(m[0, 0], Is.EqualTo(1.0).Within(G.Precision));
            Assert.That(m[0, 1], Is.EqualTo(2.0).Within(G.Precision));
        }
        
        [Test]
        public void CreateMatrix_Params_1x3()
        {
            var m = Matrix.Create(1, 3, 
                1.0, 2.0, 3.0);
            
            Assert.That(m[0, 0], Is.EqualTo(1.0).Within(G.Precision));
            Assert.That(m[0, 1], Is.EqualTo(2.0).Within(G.Precision));
            Assert.That(m[0, 2], Is.EqualTo(3.0).Within(G.Precision));
        }
        
        [Test]
        public void CreateMatrix_Params_1x4()
        {
            var m = Matrix.Create(1, 4, 
                1.0, 2.0, 3.0, 4.0);
            
            Assert.That(m[0, 0], Is.EqualTo(1.0).Within(G.Precision));
            Assert.That(m[0, 1], Is.EqualTo(2.0).Within(G.Precision));
            Assert.That(m[0, 2], Is.EqualTo(3.0).Within(G.Precision));
            Assert.That(m[0, 3], Is.EqualTo(4.0).Within(G.Precision));
        }
        
        [Test]
        public void CreateMatrix_Params_2x1()
        {
            var m = Matrix.Create(2, 1, 
                1.0, 
                2.0);
            
            Assert.That(m[0, 0], Is.EqualTo(1.0).Within(G.Precision));
            Assert.That(m[1, 0], Is.EqualTo(2.0).Within(G.Precision));
        }
        
        [Test]
        public void CreateMatrix_Params_2x2()
        {
            var m = Matrix.Create(2, 2, 
                1.0, 2.0,
                3.0, 4.0);
            
            Assert.That(m[0, 0], Is.EqualTo(1.0).Within(G.Precision));
            Assert.That(m[0, 1], Is.EqualTo(2.0).Within(G.Precision));
            
            Assert.That(m[1, 0], Is.EqualTo(3.0).Within(G.Precision));
            Assert.That(m[1, 1], Is.EqualTo(4.0).Within(G.Precision));
        }
        
        [Test]
        public void CreateMatrix_Params_2x3()
        {
            var m = Matrix.Create(2, 3, 
                1.0, 2.0, 3.0,
                4.0, 5.0, 6.0);
            
            Assert.That(m[0, 0], Is.EqualTo(1.0).Within(G.Precision));
            Assert.That(m[0, 1], Is.EqualTo(2.0).Within(G.Precision));
            Assert.That(m[0, 2], Is.EqualTo(3.0).Within(G.Precision));
            
            Assert.That(m[1, 0], Is.EqualTo(4.0).Within(G.Precision));
            Assert.That(m[1, 1], Is.EqualTo(5.0).Within(G.Precision));
            Assert.That(m[1, 2], Is.EqualTo(6.0).Within(G.Precision));
        }
        
        [Test]
        public void CreateMatrix_Params_2x4()
        {
            var m = Matrix.Create(2, 4, 
                1.0, 2.0, 3.0, 4.0,
                5.0, 6.0, 7.0, 8.0);
            
            Assert.That(m[0, 0], Is.EqualTo(1.0).Within(G.Precision));
            Assert.That(m[0, 1], Is.EqualTo(2.0).Within(G.Precision));
            Assert.That(m[0, 2], Is.EqualTo(3.0).Within(G.Precision));
            Assert.That(m[0, 3], Is.EqualTo(4.0).Within(G.Precision));
            
            Assert.That(m[1, 0], Is.EqualTo(5.0).Within(G.Precision));
            Assert.That(m[1, 1], Is.EqualTo(6.0).Within(G.Precision));
            Assert.That(m[1, 2], Is.EqualTo(7.0).Within(G.Precision));
            Assert.That(m[1, 3], Is.EqualTo(8.0).Within(G.Precision));
        }
        
        [Test]
        public void CreateMatrix_Params_3x1()
        {
            var m = Matrix.Create(3, 1, 
                1.0, 
                2.0,
                3.0);
            
            Assert.That(m[0, 0], Is.EqualTo(1.0).Within(G.Precision));
            Assert.That(m[1, 0], Is.EqualTo(2.0).Within(G.Precision));
            Assert.That(m[2, 0], Is.EqualTo(3.0).Within(G.Precision));
        }
        
        [Test]
        public void CreateMatrix_Params_3x2()
        {
            var m = Matrix.Create(3, 2, 
                1.0, 2.0, 
                3.0, 4.0,
                5.0, 6.0);
            
            Assert.That(m[0, 0], Is.EqualTo(1.0).Within(G.Precision));
            Assert.That(m[0, 1], Is.EqualTo(2.0).Within(G.Precision));
            
            Assert.That(m[1, 0], Is.EqualTo(3.0).Within(G.Precision));
            Assert.That(m[1, 1], Is.EqualTo(4.0).Within(G.Precision));
            
            Assert.That(m[2, 0], Is.EqualTo(5.0).Within(G.Precision));
            Assert.That(m[2, 1], Is.EqualTo(6.0).Within(G.Precision));
        }
        
        [Test]
        public void CreateMatrix_Params_3x3()
        {
            var m = Matrix.Create(3, 3, 
                1.0, 2.0, 3.0,
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0);
            
            Assert.That(m[0, 0], Is.EqualTo(1.0).Within(G.Precision));
            Assert.That(m[0, 1], Is.EqualTo(2.0).Within(G.Precision));
            Assert.That(m[0, 2], Is.EqualTo(3.0).Within(G.Precision));
            
            Assert.That(m[1, 0], Is.EqualTo(4.0).Within(G.Precision));
            Assert.That(m[1, 1], Is.EqualTo(5.0).Within(G.Precision));
            Assert.That(m[1, 2], Is.EqualTo(6.0).Within(G.Precision));
            
            Assert.That(m[2, 0], Is.EqualTo(7.0).Within(G.Precision));
            Assert.That(m[2, 1], Is.EqualTo(8.0).Within(G.Precision));
            Assert.That(m[2, 2], Is.EqualTo(9.0).Within(G.Precision));
        }
        
        [Test]
        public void CreateMatrix_Params_3x4()
        {
            var m = Matrix.Create(3, 4, 
                1.0, 2.0, 3.0, 4.0,
                5.0, 6.0, 7.0, 8.0,
                9.0, 10.0, 11.0, 12.0);
            
            Assert.That(m[0, 0], Is.EqualTo(1.0).Within(G.Precision));
            Assert.That(m[0, 1], Is.EqualTo(2.0).Within(G.Precision));
            Assert.That(m[0, 2], Is.EqualTo(3.0).Within(G.Precision));
            Assert.That(m[0, 3], Is.EqualTo(4.0).Within(G.Precision));
            
            Assert.That(m[1, 0], Is.EqualTo(5.0).Within(G.Precision));
            Assert.That(m[1, 1], Is.EqualTo(6.0).Within(G.Precision));
            Assert.That(m[1, 2], Is.EqualTo(7.0).Within(G.Precision));
            Assert.That(m[1, 3], Is.EqualTo(8.0).Within(G.Precision));
            
            Assert.That(m[2, 0], Is.EqualTo(9.0).Within(G.Precision));
            Assert.That(m[2, 1], Is.EqualTo(10.0).Within(G.Precision));
            Assert.That(m[2, 2], Is.EqualTo(11.0).Within(G.Precision));
            Assert.That(m[2, 3], Is.EqualTo(12.0).Within(G.Precision));
        }
        
        [Test]
        public void CreateMatrix_Params_4x1()
        {
            var m = Matrix.Create(4, 1, 
                1.0, 
                2.0,
                3.0,
                4.0);
            
            Assert.That(m[0, 0], Is.EqualTo(1.0).Within(G.Precision));
            Assert.That(m[1, 0], Is.EqualTo(2.0).Within(G.Precision));
            Assert.That(m[2, 0], Is.EqualTo(3.0).Within(G.Precision));
            Assert.That(m[3, 0], Is.EqualTo(4.0).Within(G.Precision));
        }
        
        [Test]
        public void CreateMatrix_Params_4x2()
        {
            var m = Matrix.Create(4, 2, 
                1.0, 2.0, 
                3.0, 4.0,
                5.0, 6.0,
                7.0, 8.0);
            
            Assert.That(m[0, 0], Is.EqualTo(1.0).Within(G.Precision));
            Assert.That(m[0, 1], Is.EqualTo(2.0).Within(G.Precision));
            
            Assert.That(m[1, 0], Is.EqualTo(3.0).Within(G.Precision));
            Assert.That(m[1, 1], Is.EqualTo(4.0).Within(G.Precision));
            
            Assert.That(m[2, 0], Is.EqualTo(5.0).Within(G.Precision));
            Assert.That(m[2, 1], Is.EqualTo(6.0).Within(G.Precision));
            
            Assert.That(m[3, 0], Is.EqualTo(7.0).Within(G.Precision));
            Assert.That(m[3, 1], Is.EqualTo(8.0).Within(G.Precision));
        }
        
        [Test]
        public void CreateMatrix_Params_4x3()
        {
            var m = Matrix.Create(4, 3, 
                1.0, 2.0, 3.0,
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0,
                10.0, 11.0, 12.0);
            
            Assert.That(m[0, 0], Is.EqualTo(1.0).Within(G.Precision));
            Assert.That(m[0, 1], Is.EqualTo(2.0).Within(G.Precision));
            Assert.That(m[0, 2], Is.EqualTo(3.0).Within(G.Precision));
            
            Assert.That(m[1, 0], Is.EqualTo(4.0).Within(G.Precision));
            Assert.That(m[1, 1], Is.EqualTo(5.0).Within(G.Precision));
            Assert.That(m[1, 2], Is.EqualTo(6.0).Within(G.Precision));
            
            Assert.That(m[2, 0], Is.EqualTo(7.0).Within(G.Precision));
            Assert.That(m[2, 1], Is.EqualTo(8.0).Within(G.Precision));
            Assert.That(m[2, 2], Is.EqualTo(9.0).Within(G.Precision));
            
            Assert.That(m[3, 0], Is.EqualTo(10.0).Within(G.Precision));
            Assert.That(m[3, 1], Is.EqualTo(11.0).Within(G.Precision));
            Assert.That(m[3, 2], Is.EqualTo(12.0).Within(G.Precision));
        }
        
        [Test]
        public void CreateMatrix_Params_4x4()
        {
            var m = Matrix.Create(4, 4, 
                1.0, 2.0, 3.0, 4.0,
                5.0, 6.0, 7.0, 8.0,
                9.0, 10.0, 11.0, 12.0,
                13.0, 14.0, 15.0, 16.0);
            
            Assert.That(m[0, 0], Is.EqualTo(1.0).Within(G.Precision));
            Assert.That(m[0, 1], Is.EqualTo(2.0).Within(G.Precision));
            Assert.That(m[0, 2], Is.EqualTo(3.0).Within(G.Precision));
            Assert.That(m[0, 3], Is.EqualTo(4.0).Within(G.Precision));
            
            Assert.That(m[1, 0], Is.EqualTo(5.0).Within(G.Precision));
            Assert.That(m[1, 1], Is.EqualTo(6.0).Within(G.Precision));
            Assert.That(m[1, 2], Is.EqualTo(7.0).Within(G.Precision));
            Assert.That(m[1, 3], Is.EqualTo(8.0).Within(G.Precision));
            
            Assert.That(m[2, 0], Is.EqualTo(9.0).Within(G.Precision));
            Assert.That(m[2, 1], Is.EqualTo(10.0).Within(G.Precision));
            Assert.That(m[2, 2], Is.EqualTo(11.0).Within(G.Precision));
            Assert.That(m[2, 3], Is.EqualTo(12.0).Within(G.Precision));
            
            Assert.That(m[3, 0], Is.EqualTo(13.0).Within(G.Precision));
            Assert.That(m[3, 1], Is.EqualTo(14.0).Within(G.Precision));
            Assert.That(m[3, 2], Is.EqualTo(15.0).Within(G.Precision));
            Assert.That(m[3, 3], Is.EqualTo(16.0).Within(G.Precision));
        }

        [Test]
        public void GetElements_Count(
            [Values(0, 1, 2, 3, 4)] int rows,
            [Values(0, 1, 2, 3, 4)] int cols)
        {
            var m = Matrix.Create(rows, cols);

            var expectedCount = rows * cols;
            Assert.That(m.GetElements().Count(), Is.EqualTo(expectedCount));
        }
        
        [Test]
        public void CreateMatrix_With_ElementsArray(
            [Values(1, 2, 3, 4)] int rows,
            [Values(1, 2, 3, 4)] int cols)
        {
            var elementCount = rows * cols;
            var elements = Enumerable.Range(0, elementCount).Select(x => (double)x).ToArray();

            var m = Matrix.Create(rows, cols, elements);
            
            Assert.That(m.rows, Is.EqualTo(rows));
            Assert.That(m.cols, Is.EqualTo(cols));
            Assert.That(m.GetElements(), Is.EqualTo(elements).AsCollection.Within(G.Precision));
        }
        
        #endregion CreateMatrix(rows, cols, params double[] values)
        
        #region Equals
        
        [Test]
        public void IsEqual(
            [Values(0, 1, 2, 3, 4)] int rows,
            [Values(0, 1, 2, 3, 4)] int cols)
        {
            var elementCount = rows * cols;
            var elements = Enumerable.Range(0, elementCount).Select(x => (double)x).ToArray();
            
            var a = Matrix.Create(rows, cols, elements);
            var b = Matrix.Create(rows, cols, elements);
            
            Assert.That(a, Is.EqualTo(b));
        }
        
        [Test]
        public void IsNotEqual(
            [Values(1, 2, 3, 4)] int rows,
            [Values(1, 2, 3, 4)] int cols)
        {
            var elementCount = rows * cols;
            var elements = Enumerable.Range(0, elementCount).Select(x => (double)x).ToArray();
            
            var a = Matrix.Create(rows, cols, elements);
            var b = Matrix.Create(rows, cols, elements);
            b[0, 0] = -1.0;
            
            Assert.That(a, Is.Not.EqualTo(b));
        }
        
        [Test]
        public void IsNotEqual_With_null(
            [Values(1, 2, 3, 4)] int rows,
            [Values(1, 2, 3, 4)] int cols)
        {
            var a = new Matrix(rows, cols);
            Matrix b = null;
            
            Assert.That(a, Is.Not.EqualTo(b));
            Assert.That(a.Equals(b), Is.False);
        }
        
        [Test]
        public void IsNotEqual_With_DifferentRowCount(
            [Values(1, 2, 3, 4)] int rows,
            [Values(1, 2, 3, 4)] int cols)
        {
            var a = new Matrix(rows + 1, cols);
            var b = new Matrix(rows, cols);
            
            Assert.That(a, Is.Not.EqualTo(b));
            Assert.That(a.Equals(b), Is.False);
        }
        
        [Test]
        public void IsNotEqual_With_DifferentColumnCount(
            [Values(1, 2, 3, 4)] int rows,
            [Values(1, 2, 3, 4)] int cols)
        {
            var a = new Matrix(rows, cols + 1);
            var b = new Matrix(rows, cols);
            
            Assert.That(a, Is.Not.EqualTo(b));
            Assert.That(a.Equals(b), Is.False);
        }
        
        #endregion Equals

        #region GetCol(idx)
        
        [Test]
        public void GetCol_EmptyCol_With_0x0()
        {
            var m = Matrix.Create(0, 0);

            var col = m.GetCol(0);
            
            var expected = Matrix.Create(0, 1);
            Assert.That(col, Is.EqualTo(expected));
        }

        [Test]
        public void GetCol_1x1()
        {
            var m = Matrix.Create(1, 1,
                1.0);
            
            // Column 0
            var expected = Matrix.Create(1, 1,
                1.0);
            Assert.That(m.GetCol(0), Is.EqualTo(expected));
        }

        [Test]
        public void GetCol_1x2()
        {
            var m = Matrix.Create(1, 2,
                1.0, 2.0);

            // Column 0
            {
                var expected = Matrix.Create(1, 1,
                    1.0);
                Assert.That(m.GetCol(0), Is.EqualTo(expected));
            }
            // Column 1
            {
                var expected = Matrix.Create(1, 1,
                    2.0);
                Assert.That(m.GetCol(1), Is.EqualTo(expected));
            }
        }

        [Test]
        public void GetCol_1x3()
        {
            var m = Matrix.Create(1, 3,
                1.0, 2.0, 3.0);

            // Column 0
            {
                var expected = Matrix.Create(1, 1,
                    1.0);
                Assert.That(m.GetCol(0), Is.EqualTo(expected));
            }
            // Column 1
            {
                var expected = Matrix.Create(1, 1,
                    2.0);
                Assert.That(m.GetCol(1), Is.EqualTo(expected));
            }
            // Column 2
            {
                var expected = Matrix.Create(1, 1,
                    3.0);
                Assert.That(m.GetCol(2), Is.EqualTo(expected));
            }
        }

        [Test]
        public void GetCol_1x4()
        {
            var m = Matrix.Create(1, 4,
                1.0, 2.0, 3.0, 4.0);

            // Column 0
            {
                var expected = Matrix.Create(1, 1,
                    1.0);
                Assert.That(m.GetCol(0), Is.EqualTo(expected));
            }
            // Column 1
            {
                var expected = Matrix.Create(1, 1,
                    2.0);
                Assert.That(m.GetCol(1), Is.EqualTo(expected));
            }
            // Column 2
            {
                var expected = Matrix.Create(1, 1,
                    3.0);
                Assert.That(m.GetCol(2), Is.EqualTo(expected));
            }
            // Column 3
            {
                var expected = Matrix.Create(1, 1,
                    4.0);
                Assert.That(m.GetCol(3), Is.EqualTo(expected));
            }
        }

        [Test]
        public void GetCol_2x1()
        {
            var m = Matrix.Create(2, 1,
                1.0,
                2.0);
            
            // Column 0
            var expected = Matrix.Create(2, 1,
                1.0, 
                2.0);
            Assert.That(m.GetCol(0), Is.EqualTo(expected));
        }

        [Test]
        public void GetCol_2x2()
        {
            var m = Matrix.Create(2, 2,
                1.0, 2.0,
                3.0, 4.0);

            // Column 0
            {
                var expected = Matrix.Create(2, 1,
                    1.0,
                    3.0);
                Assert.That(m.GetCol(0), Is.EqualTo(expected));
            }
            // Column 1
            {
                var expected = Matrix.Create(2, 1,
                    2.0,
                    4.0);
                Assert.That(m.GetCol(1), Is.EqualTo(expected));
            }
        }

        [Test]
        public void GetCol_2x3()
        {
            var m = Matrix.Create(2, 3,
                1.0, 2.0, 3.0,
                4.0, 5.0, 6.0);

            // Column 0
            {
                var expected = Matrix.Create(2, 1,
                    1.0,
                    4.0);
                Assert.That(m.GetCol(0), Is.EqualTo(expected));
            }
            // Column 1
            {
                var expected = Matrix.Create(2, 1,
                    2.0,
                    5.0);
                Assert.That(m.GetCol(1), Is.EqualTo(expected));
            }
            // Column 2
            {
                var expected = Matrix.Create(2, 1,
                    3.0,
                    6.0);
                Assert.That(m.GetCol(2), Is.EqualTo(expected));
            }
        }

        [Test]
        public void GetCol_2x4()
        {
            var m = Matrix.Create(2, 4,
                1.0, 2.0, 3.0, 4.0,
                5.0, 6.0, 7.0, 8.0);

            // Column 0
            {
                var expected = Matrix.Create(2, 1,
                    1.0,
                    5.0);
                Assert.That(m.GetCol(0), Is.EqualTo(expected));
            }
            // Column 1
            {
                var expected = Matrix.Create(2, 1,
                    2.0,
                    6.0);
                Assert.That(m.GetCol(1), Is.EqualTo(expected));
            }
            // Column 2
            {
                var expected = Matrix.Create(2, 1,
                    3.0,
                    7.0);
                Assert.That(m.GetCol(2), Is.EqualTo(expected));
            }
            // Column 3
            {
                var expected = Matrix.Create(2, 1,
                    4.0,
                    8.0);
                Assert.That(m.GetCol(3), Is.EqualTo(expected));
            }
        }

        [Test]
        public void GetCol_3x1()
        {
            var m = Matrix.Create(3, 1,
                1.0,
                2.0,
                3.0);
            
            // Column 0
            var expected = Matrix.Create(3, 1,
                1.0, 
                2.0,
                3.0);
            Assert.That(m.GetCol(0), Is.EqualTo(expected));
        }

        [Test]
        public void GetCol_3x2()
        {
            var m = Matrix.Create(3, 2,
                1.0, 2.0,
                3.0, 4.0,
                5.0, 6.0);
            
            // Column 0
            {
                var expected = Matrix.Create(3, 1,
                    1.0,
                    3.0,
                    5.0);
                Assert.That(m.GetCol(0), Is.EqualTo(expected));
            }
            // Column 1
            {
                var expected = Matrix.Create(3, 1,
                    2.0,
                    4.0,
                    6.0);
                Assert.That(m.GetCol(1), Is.EqualTo(expected));
            }
        }

        [Test]
        public void GetCol_3x3()
        {
            var m = Matrix.Create(3, 3,
                1.0, 2.0, 3.0, 
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0);
            
            // Column 0
            {
                var expected = Matrix.Create(3, 1,
                    1.0,
                    4.0,
                    7.0);
                Assert.That(m.GetCol(0), Is.EqualTo(expected));
            }
            // Column 1
            {
                var expected = Matrix.Create(3, 1,
                    2.0,
                    5.0,
                    8.0);
                Assert.That(m.GetCol(1), Is.EqualTo(expected));
            }
            // Column 2
            {
                var expected = Matrix.Create(3, 1,
                    3.0,
                    6.0,
                    9.0);
                Assert.That(m.GetCol(2), Is.EqualTo(expected));
            }
        }

        [Test]
        public void GetCol_3x4()
        {
            var m = Matrix.Create(3, 4,
                1.0, 2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0, 8.0, 
                9.0, 10.0, 11.0, 12.0);
            
            // Column 0
            {
                var expected = Matrix.Create(3, 1,
                    1.0,
                    5.0,
                    9.0);
                Assert.That(m.GetCol(0), Is.EqualTo(expected));
            }
            // Column 1
            {
                var expected = Matrix.Create(3, 1,
                    2.0,
                    6.0,
                    10.0);
                Assert.That(m.GetCol(1), Is.EqualTo(expected));
            }
            // Column 2
            {
                var expected = Matrix.Create(3, 1,
                    3.0,
                    7.0,
                    11.0);
                Assert.That(m.GetCol(2), Is.EqualTo(expected));
            }
            // Column 3
            {
                var expected = Matrix.Create(3, 1,
                    4.0,
                    8.0,
                    12.0);
                Assert.That(m.GetCol(3), Is.EqualTo(expected));
            }
        }

        [Test]
        public void GetCol_4x1()
        {
            var m = Matrix.Create(4, 1,
                1.0,
                2.0,
                3.0,
                4.0);
            
            // Column 0
            var expected = Matrix.Create(4, 1,
                1.0, 
                2.0,
                3.0,
                4.0);
            Assert.That(m.GetCol(0), Is.EqualTo(expected));
        }

        [Test]
        public void GetCol_4x2()
        {
            var m = Matrix.Create(4, 2,
                1.0, 2.0,
                3.0, 4.0,
                5.0, 6.0,
                7.0, 8.0);
            
            // Column 0
            {
                var expected = Matrix.Create(4, 1,
                    1.0,
                    3.0,
                    5.0,
                    7.0);
                Assert.That(m.GetCol(0), Is.EqualTo(expected));
            }
            // Column 1
            {
                var expected = Matrix.Create(4, 1,
                    2.0,
                    4.0,
                    6.0,
                    8.0);
                Assert.That(m.GetCol(1), Is.EqualTo(expected));
            }
        }

        [Test]
        public void GetCol_4x3()
        {
            var m = Matrix.Create(4, 3,
                1.0, 2.0, 3.0,
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0,
                10.0, 11.0, 12.0);
            
            // Column 0
            {
                var expected = Matrix.Create(4, 1,
                    1.0,
                    4.0,
                    7.0,
                    10.0);
                Assert.That(m.GetCol(0), Is.EqualTo(expected));
            }
            // Column 1
            {
                var expected = Matrix.Create(4, 1,
                    2.0,
                    5.0,
                    8.0,
                    11.0);
                Assert.That(m.GetCol(1), Is.EqualTo(expected));
            }
            // Column 2
            {
                var expected = Matrix.Create(4, 1,
                    3.0,
                    6.0,
                    9.0,
                    12.0);
                Assert.That(m.GetCol(2), Is.EqualTo(expected));
            }
        }

        [Test]
        public void GetCol_4x4()
        {
            var m = Matrix.Create(4, 4,
                1.0, 2.0, 3.0, 4.0,
                5.0, 6.0, 7.0, 8.0,
                9.0, 10.0, 11.0, 12.0,
                13.0, 14.0, 15.0, 16.0);
            
            // Column 0
            {
                var expected = Matrix.Create(4, 1,
                    1.0,
                    5.0,
                    9.0,
                    13.0);
                Assert.That(m.GetCol(0), Is.EqualTo(expected));
            }
            // Column 1
            {
                var expected = Matrix.Create(4, 1,
                    2.0,
                    6.0,
                    10.0,
                    14.0);
                Assert.That(m.GetCol(1), Is.EqualTo(expected));
            }
            // Column 2
            {
                var expected = Matrix.Create(4, 1,
                    3.0,
                    7.0,
                    11.0,
                    15.0);
                Assert.That(m.GetCol(2), Is.EqualTo(expected));
            }
            // Column 3
            {
                var expected = Matrix.Create(4, 1,
                    4.0,
                    8.0,
                    12.0,
                    16.0);
                Assert.That(m.GetCol(3), Is.EqualTo(expected));
            }
        }
        
        #endregion GetCol(idx)
        
        #region SetCol(col, idx)
        
        [Test]
        public void SetCol_1x1()
        {
            var col = Matrix.Create(1, 1,
                1.0);
            
            // Column 0
            var m = Matrix.Create(1, 1);
            m.SetCol(col, 0);
            
            Assert.That(m, Is.EqualTo(Matrix.Create(1, 1,
                1.0)));
        }

        [Test]
        public void SetCol_1x2()
        {
            var col = Matrix.Create(1, 1,
                1.0);
            
            // Column 0
            {
                var m = Matrix.Create(1, 2);
                m.SetCol(col, 0);
                    
                Assert.That(m, Is.EqualTo(Matrix.Create(1, 2,
                    1.0, 0.0)));
            }
            // Column 1
            {
                var m = Matrix.Create(1, 2);
                m.SetCol(col, 1);
                
                Assert.That(m, Is.EqualTo(Matrix.Create(1, 2,
                    0.0, 1.0)));
            }
        }

        [Test]
        public void SetCol_1x3()
        {
            var col = Matrix.Create(1, 1,
                1.0);
            
            // Column 0
            {
                var m = Matrix.Create(1, 3);
                m.SetCol(col, 0);
                    
                Assert.That(m, Is.EqualTo(Matrix.Create(1, 3,
                    1.0, 0.0, 0.0)));
            }
            // Column 1
            {
                var m = Matrix.Create(1, 3);
                m.SetCol(col, 1);
                
                Assert.That(m, Is.EqualTo(Matrix.Create(1, 3,
                    0.0, 1.0, 0.0)));
            }
            // Column 2
            {
                var m = Matrix.Create(1, 3);
                m.SetCol(col, 2);
                
                Assert.That(m, Is.EqualTo(Matrix.Create(1, 3,
                    0.0, 0.0, 1.0)));
            }
        }

        [Test]
        public void SetCol_1x4()
        {
            var col = Matrix.Create(1, 1,
                1.0);
            
            // Column 0
            {
                var m = Matrix.Create(1, 4);
                m.SetCol(col, 0);
                    
                Assert.That(m, Is.EqualTo(Matrix.Create(1, 4,
                    1.0, 0.0, 0.0, 0.0)));
            }
            // Column 1
            {
                var m = Matrix.Create(1, 4);
                m.SetCol(col, 1);
                
                Assert.That(m, Is.EqualTo(Matrix.Create(1, 4,
                    0.0, 1.0, 0.0, 0.0)));
            }
            // Column 2
            {
                var m = Matrix.Create(1, 4);
                m.SetCol(col, 2);
                
                Assert.That(m, Is.EqualTo(Matrix.Create(1, 4,
                    0.0, 0.0, 1.0, 0.0)));
            }
            // Column 3
            {
                var m = Matrix.Create(1, 4);
                m.SetCol(col, 3);
                
                Assert.That(m, Is.EqualTo(Matrix.Create(1, 4,
                    0.0, 0.0, 0.0, 1.0)));
            }
        }

        [Test]
        public void SetCol_2x1()
        {
            var col = Matrix.Create(2, 1,
                1.0,
                2.0);
            
            // Column 0
            var m = Matrix.Create(2, 1);
            m.SetCol(col, 0);
            
            Assert.That(m, Is.EqualTo(Matrix.Create(2, 1,
                1.0,
                2.0)));
        }

        [Test]
        public void SetCol_2x2()
        {
            var col = Matrix.Create(2, 1,
                1.0,
                2.0);
            
            // Column 0
            {
                var m = Matrix.Create(2, 2);
                m.SetCol(col, 0);

                Assert.That(m, Is.EqualTo(Matrix.Create(2, 2,
                    1.0, 0.0,
                    2.0, 0.0)));
            }
            // Column 1
            {
                var m = Matrix.Create(2, 2);
                m.SetCol(col, 1);

                Assert.That(m, Is.EqualTo(Matrix.Create(2, 2,
                    0.0, 1.0,
                    0.0, 2.0)));
            }
        }

        [Test]
        public void SetCol_2x3()
        {
            var col = Matrix.Create(2, 1,
                1.0,
                2.0);
            
            // Column 0
            {
                var m = Matrix.Create(2, 3);
                m.SetCol(col, 0);

                Assert.That(m, Is.EqualTo(Matrix.Create(2, 3,
                    1.0, 0.0, 0.0,
                    2.0, 0.0, 0.0)));
            }
            // Column 1
            {
                var m = Matrix.Create(2, 3);
                m.SetCol(col, 1);

                Assert.That(m, Is.EqualTo(Matrix.Create(2, 3,
                    0.0, 1.0, 0.0,
                    0.0, 2.0, 0.0)));
            }
            // Column 2
            {
                var m = Matrix.Create(2, 3);
                m.SetCol(col, 2);

                Assert.That(m, Is.EqualTo(Matrix.Create(2, 3,
                    0.0, 0.0, 1.0,
                    0.0, 0.0, 2.0)));
            }
        }

        [Test]
        public void SetCol_2x4()
        {
            var col = Matrix.Create(2, 1,
                1.0,
                2.0);
            
            // Column 0
            {
                var m = Matrix.Create(2, 4);
                m.SetCol(col, 0);

                Assert.That(m, Is.EqualTo(Matrix.Create(2, 4,
                    1.0, 0.0, 0.0, 0.0,
                    2.0, 0.0, 0.0, 0.0)));
            }
            // Column 1
            {
                var m = Matrix.Create(2, 4);
                m.SetCol(col, 1);

                Assert.That(m, Is.EqualTo(Matrix.Create(2, 4,
                    0.0, 1.0, 0.0, 0.0,
                    0.0, 2.0, 0.0, 0.0)));
            }
            // Column 2
            {
                var m = Matrix.Create(2, 4);
                m.SetCol(col, 2);

                Assert.That(m, Is.EqualTo(Matrix.Create(2, 4,
                    0.0, 0.0, 1.0, 0.0,
                    0.0, 0.0, 2.0, 0.0)));
            }
            // Column 3
            {
                var m = Matrix.Create(2, 4);
                m.SetCol(col, 3);

                Assert.That(m, Is.EqualTo(Matrix.Create(2, 4,
                    0.0, 0.0, 0.0, 1.0,
                    0.0, 0.0, 0.0, 2.0)));
            }
        }

        [Test]
        public void SetCol_3x1()
        {
            var col = Matrix.Create(3, 1,
                1.0,
                2.0,
                3.0);
            
            // Column 0
            var m = Matrix.Create(3, 1);
            m.SetCol(col, 0);
            
            Assert.That(m, Is.EqualTo(Matrix.Create(3, 1,
                1.0,
                2.0,
                3.0)));
        }

        [Test]
        public void SetCol_3x2()
        {
            var col = Matrix.Create(3, 1,
                1.0,
                2.0,
                3.0);
            
            // Column 0
            {
                var m = Matrix.Create(3, 2);
                m.SetCol(col, 0);

                Assert.That(m, Is.EqualTo(Matrix.Create(3, 2,
                    1.0, 0.0,
                    2.0, 0.0,
                    3.0, 0.0)));
            }
            // Column 1
            {
                var m = Matrix.Create(3, 2);
                m.SetCol(col, 1);

                Assert.That(m, Is.EqualTo(Matrix.Create(3, 2,
                    0.0, 1.0,
                    0.0, 2.0,
                    0.0, 3.0)));
            }
        }

        [Test]
        public void SetCol_3x3()
        {
            var col = Matrix.Create(3, 1,
                1.0,
                2.0,
                3.0);
            
            // Column 0
            {
                var m = Matrix.Create(3, 3);
                m.SetCol(col, 0);

                Assert.That(m, Is.EqualTo(Matrix.Create(3, 3,
                    1.0, 0.0, 0.0,
                    2.0, 0.0, 0.0,
                    3.0, 0.0, 0.0)));
            }
            // Column 1
            {
                var m = Matrix.Create(3, 3);
                m.SetCol(col, 1);

                Assert.That(m, Is.EqualTo(Matrix.Create(3, 3,
                    0.0, 1.0, 0.0,
                    0.0, 2.0, 0.0,
                    0.0, 3.0, 0.0)));
            }
            // Column 2
            {
                var m = Matrix.Create(3, 3);
                m.SetCol(col, 2);

                Assert.That(m, Is.EqualTo(Matrix.Create(3, 3,
                    0.0, 0.0, 1.0,
                    0.0, 0.0, 2.0,
                    0.0, 0.0, 3.0)));
            }
        }

        [Test]
        public void SetCol_3x4()
        {
            var col = Matrix.Create(3, 1,
                1.0,
                2.0,
                3.0);
            
            // Column 0
            {
                var m = Matrix.Create(3, 4);
                m.SetCol(col, 0);

                Assert.That(m, Is.EqualTo(Matrix.Create(3, 4,
                    1.0, 0.0, 0.0, 0.0,
                    2.0, 0.0, 0.0, 0.0,
                    3.0, 0.0, 0.0, 0.0)));
            }
            // Column 1
            {
                var m = Matrix.Create(3, 4);
                m.SetCol(col, 1);

                Assert.That(m, Is.EqualTo(Matrix.Create(3, 4,
                    0.0, 1.0, 0.0, 0.0,
                    0.0, 2.0, 0.0, 0.0,
                    0.0, 3.0, 0.0, 0.0)));
            }
            // Column 2
            {
                var m = Matrix.Create(3, 4);
                m.SetCol(col, 2);

                Assert.That(m, Is.EqualTo(Matrix.Create(3, 4,
                    0.0, 0.0, 1.0, 0.0,
                    0.0, 0.0, 2.0, 0.0,
                    0.0, 0.0, 3.0, 0.0)));
            }
            // Column 3
            {
                var m = Matrix.Create(3, 4);
                m.SetCol(col, 3);

                Assert.That(m, Is.EqualTo(Matrix.Create(3, 4,
                    0.0, 0.0, 0.0, 1.0,
                    0.0, 0.0, 0.0, 2.0,
                    0.0, 0.0, 0.0, 3.0)));
            }
        }

        [Test]
        public void SetCol_4x1()
        {
            var col = Matrix.Create(4, 1,
                1.0,
                2.0,
                3.0,
                4.0);
            
            // Column 0
            var m = Matrix.Create(4, 1);
            m.SetCol(col, 0);
            
            Assert.That(m, Is.EqualTo(Matrix.Create(4, 1,
                1.0,
                2.0,
                3.0,
                4.0)));
        }

        [Test]
        public void SetCol_4x2()
        {
            var col = Matrix.Create(4, 1,
                1.0,
                2.0,
                3.0,
                4.0);
            
            // Column 0
            {
                var m = Matrix.Create(4, 2);
                m.SetCol(col, 0);

                Assert.That(m, Is.EqualTo(Matrix.Create(4, 2,
                    1.0, 0.0,
                    2.0, 0.0,
                    3.0, 0.0,
                    4.0, 0.0)));
            }
            // Column 1
            {
                var m = Matrix.Create(4, 2);
                m.SetCol(col, 1);

                Assert.That(m, Is.EqualTo(Matrix.Create(4, 2,
                    0.0, 1.0,
                    0.0, 2.0,
                    0.0, 3.0,
                    0.0, 4.0)));
            }
        }

        [Test]
        public void SetCol_4x3()
        {
            var col = Matrix.Create(4, 1,
                1.0,
                2.0,
                3.0,
                4.0);
            
            // Column 0
            {
                var m = Matrix.Create(4, 3);
                m.SetCol(col, 0);

                Assert.That(m, Is.EqualTo(Matrix.Create(4, 3,
                    1.0, 0.0, 0.0,
                    2.0, 0.0, 0.0,
                    3.0, 0.0, 0.0,
                    4.0, 0.0, 0.0)));
            }
            // Column 1
            {
                var m = Matrix.Create(4, 3);
                m.SetCol(col, 1);

                Assert.That(m, Is.EqualTo(Matrix.Create(4, 3,
                    0.0, 1.0, 0.0,
                    0.0, 2.0, 0.0,
                    0.0, 3.0, 0.0,
                    0.0, 4.0, 0.0)));
            }
            // Column 2
            {
                var m = Matrix.Create(4, 3);
                m.SetCol(col, 2);

                Assert.That(m, Is.EqualTo(Matrix.Create(4, 3,
                    0.0, 0.0, 1.0,
                    0.0, 0.0, 2.0,
                    0.0, 0.0, 3.0,
                    0.0, 0.0, 4.0)));
            }
        }
        
        [Test]
        public void SetCol_4x4()
        {
            var col = Matrix.Create(4, 1,
                1.0,
                2.0,
                3.0,
                4.0);
            
            // Column 0
            {
                var m = Matrix.Create(4, 4);
                m.SetCol(col, 0);

                Assert.That(m, Is.EqualTo(Matrix.Create(4, 4,
                    1.0, 0.0, 0.0, 0.0,
                    2.0, 0.0, 0.0, 0.0,
                    3.0, 0.0, 0.0, 0.0,
                    4.0, 0.0, 0.0, 0.0)));
            }
            // Column 1
            {
                var m = Matrix.Create(4, 4);
                m.SetCol(col, 1);

                Assert.That(m, Is.EqualTo(Matrix.Create(4, 4,
                    0.0, 1.0, 0.0, 0.0,
                    0.0, 2.0, 0.0, 0.0,
                    0.0, 3.0, 0.0, 0.0,
                    0.0, 4.0, 0.0, 0.0)));
            }
            // Column 2
            {
                var m = Matrix.Create(4, 4);
                m.SetCol(col, 2);

                Assert.That(m, Is.EqualTo(Matrix.Create(4, 4,
                    0.0, 0.0, 1.0, 0.0,
                    0.0, 0.0, 2.0, 0.0,
                    0.0, 0.0, 3.0, 0.0,
                    0.0, 0.0, 4.0, 0.0)));
            }
            // Column 3
            {
                var m = Matrix.Create(4, 4);
                m.SetCol(col, 3);

                Assert.That(m, Is.EqualTo(Matrix.Create(4, 4,
                    0.0, 0.0, 0.0, 1.0,
                    0.0, 0.0, 0.0, 2.0,
                    0.0, 0.0, 0.0, 3.0,
                    0.0, 0.0, 0.0, 4.0)));
            }
        }
        
        #endregion SetCol(col, idx)

        #region Determinant of matrix
        
        [Test]
        public void Det_1x1()
        {
            var m = Matrix.Create(1, 1,
                5.0);
            
            Assert.That(m.Det(), Is.EqualTo(5.0).Within(G.Precision));
        }

        [Test]
        public void Det_2x2()
        {
            var m = Matrix.Create(2, 2,
                1.0, 2.0,
                3.0, 4.0);
            
            // (1*4) - (2*3) = 4 - 6 = -2
            Assert.That(m.Det(), Is.EqualTo(-2.0).Within(G.Precision));
        }

        [Test]
        public void Det_3x3()
        {
            var m = Matrix.Create(3, 3,
                1.0, 2.0, 3.0,
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0);

            // 1*(5*9-6*8) - 2*(4*9-6*7) + 3*(4*8-5*7)
            // ==   1*(-3) - 2*(-6)      + 3*(-3)
            // == -3 + 12 - 9
            // ==           0
            Assert.That(m.Det(), Is.EqualTo(0.0).Within(G.Precision));
        }

        [Test]
        public void Det_4x4()
        {
            var m = Matrix.Create(4, 4,
                1.0, 2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0, 8.0, 
                9.0, 10.0, 11.0, 12.0,
                13.0, 14.0, 15.0, 16.0);

            Assert.That(m.Det(), Is.EqualTo(0.0).Within(G.Precision));
        }

        #endregion Determinant of matrix
        
        #region Invert matrix
        
        [Test]
        public void Invert_1x1()
        {
            {
                var m = Matrix.Create(1, 1,
                    1.0);

                Assert.That(m.Invert(), Is.EqualTo(Matrix.Create(1, 1,
                    1.0)));
            }
            // Another values
            {
                var m = Matrix.Create(1, 1,
                    5.0);

                Assert.That(m.Invert(), Is.EqualTo(Matrix.Create(1, 1,
                    1.0 / 5.0)));
            }
        }

        [Test]
        public void Invert_2x2()
        {
            {
                var m = Matrix.Create(2, 2,
                    1.0, 2.0,
                    3.0, 4.0);

                Assert.That(m.Invert(), Is.EqualTo(Matrix.Create(2, 2,
                    -2.0, 1.0,
                    1.5, -0.5)));
            }
            // Identity
            {
                var m = Matrix.Create(2, 2,
                    1.0, 0.0,
                    0.0, 1.0);

                Assert.That(m.Invert(), Is.EqualTo(Matrix.Create(2, 2,
                    1.0, 0.0,
                    0.0, 1.0)));
            }
            // Another values
            {
                var m = Matrix.Create(2, 2,
                    4.0, 2.0,
                    1.0, 0.0);

                Assert.That(m.Invert(), Is.EqualTo(Matrix.Create(2, 2,
                    0.0, 1.0,
                    0.5, -2.0)));
            }
        }

        [Test]
        public void Invert_3x3()
        {
            {
                var m = Matrix.Create(3, 3,
                    1.0, 2.0, 3.0, 
                    0.0, 1.0, 4.0,
                    5.0, 6.0, 0.0);

                Assert.That(m.Invert(), Is.EqualTo(Matrix.Create(3, 3,
                    -24.0, 18.0, 5.0, 
                    20.0, -15.0, -4.0,
                    -5.0, 4.0, 1.0)));
            }
            // Identity
            {
                var m = Matrix.Create(3, 3,
                    1.0, 0.0, 0.0,
                    0.0, 1.0, 0.0,
                    0.0, 0.0, 1.0);

                Assert.That(m.Invert(), Is.EqualTo(Matrix.Create(3, 3,
                    1.0, 0.0, 0.0,
                    0.0, 1.0, 0.0,
                    0.0, 0.0, 1.0)));
            }
        }

        [Test]
        public void Invert_4x4()
        {
            // Identity
            {
                var m = Matrix.Create(4, 4,
                    1.0, 0.0, 0.0, 0.0,
                    0.0, 1.0, 0.0, 0.0,
                    0.0, 0.0, 1.0, 0.0,
                    0.0, 0.0, 0.0, 1.0);

                Assert.That(m.Invert(), Is.EqualTo(Matrix.Create(4, 4,
                    1.0, 0.0, 0.0, 0.0,
                    0.0, 1.0, 0.0, 0.0,
                    0.0, 0.0, 1.0, 0.0,
                    0.0, 0.0, 0.0, 1.0)));
            }
        }

        #endregion Invert matrix
        
        #region Duplicate matrix
        
        [Test]
        public void Duplicate()
        {
            // Identity
            {
                var m = Matrix.Create(4, 4,
                    1.0, 0.0, 0.0, 0.0,
                    0.0, 1.0, 0.0, 0.0,
                    0.0, 0.0, 1.0, 0.0,
                    0.0, 0.0, 0.0, 1.0);

                var d = m.Duplicate();
                
                Assert.That(d == m, Is.False); // reference is not equal
                Assert.That(d, Is.EqualTo(m));
            }
            // Example
            {
                var m = Matrix.Create(4, 4,
                    1.0, 2.0, 3.0, 4.0,
                    5.0, 6.0, 7.0, 8.0,
                    9.0, 10.0, 11.0, 12.0,
                    13.0, 14.0, 15.0, 16.0);

                var d = m.Duplicate();
                
                Assert.That(d == m, Is.False); // reference is not equal
                Assert.That(d, Is.EqualTo(m));
            }
        }

        #endregion Duplicate matrix
        
        #region ZeroMatrix(rows, cols)
        
        [Test]
        public void ZeroMatrix(
            [Values(0, 1, 2, 3, 4)] int rows,
            [Values(0, 1, 2, 3, 4)] int cols)
        {
            var zeroMatrix = Matrix.ZeroMatrix(rows, cols);
            
            var expected = Matrix.Create(rows, cols);
            Assert.That(zeroMatrix, Is.EqualTo(expected));
        }
        
        #endregion ZeroMatrix(rows, cols)
        
        #region IdentityMatrix(rows, cols)
        
        [Test]
        public void IdentityMatrix_0x0()
        {
            var m = Matrix.IdentityMatrix(0, 0);
            
            Assert.That(m, Is.EqualTo(Matrix.Create(0, 0)));
        }

        [Test]
        public void IdentityMatrix_1x1()
        {
            var m = Matrix.IdentityMatrix(1, 1);
            
            Assert.That(m, Is.EqualTo(Matrix.Create(1, 1,
                1.0)));
        }

        [Test]
        public void IdentityMatrix_2x2()
        {
            var m = Matrix.IdentityMatrix(2, 2);
            
            Assert.That(m, Is.EqualTo(Matrix.Create(2, 2,
                1.0, 0.0,
                0.0, 1.0)));
        }

        [Test]
        public void IdentityMatrix_3x3()
        {
            var m = Matrix.IdentityMatrix(3, 3);
            
            Assert.That(m, Is.EqualTo(Matrix.Create(3, 3,
                1.0, 0.0, 0.0,
                0.0, 1.0, 0.0,
                0.0, 0.0, 1.0)));
        }

        [Test]
        public void IdentityMatrix_4x4()
        {
            var m = Matrix.IdentityMatrix(4, 4);
            
            Assert.That(m, Is.EqualTo(Matrix.Create(4, 4,
                1.0, 0.0, 0.0, 0.0,
                0.0, 1.0, 0.0, 0.0,
                0.0, 0.0, 1.0, 0.0,
                0.0, 0.0, 0.0, 1.0)));
        }

        #endregion IdentityMatrix(rows, cols)
        
        #region Transpose matrix
        
        [Test]
        public void Transpose_Square_ZeroMatrix(
            [Values(0, 1, 2, 3, 4, 999)] int size)
        {
            var m = Matrix.ZeroMatrix(size, size);
            
            var transpose = Matrix.Transpose(m);

            Assert.That(transpose != m, Is.True);  // Check if it is new object (different reference)
            Assert.That(transpose, Is.EqualTo(m)); // Check if it has same values 
        }

        [Test]
        public void Transpose_Square_IdentityMatrix(
            [Values(0, 1, 2, 3, 4, 999)] int size)
        {
            var m = Matrix.IdentityMatrix(size, size);
            
            var transpose = Matrix.Transpose(m);

            Assert.That(transpose != m, Is.True);  // Check if it is new object (different reference)
            Assert.That(transpose, Is.EqualTo(m)); // Check if it has same values 
        }

        [Test]
        public void Transpose_NonSquare(
            [Values(0, 1, 2, 3, 4, 999)] int rows,
            [Values(0, 1, 2, 3, 4, 999)] int cols)
        {
            var m = Matrix.Create(rows, cols);
            
            var transpose = Matrix.Transpose(m);

            Assert.That(transpose != m, Is.True);  // Check if it is new object (different reference)
            
            Assert.That(transpose.cols, Is.EqualTo(rows));
            Assert.That(transpose.rows, Is.EqualTo(cols));
        }

        [Test]
        public void Transpose_0x0()
        {
            var m = Matrix.Create(0, 0);
            
            Assert.That(Matrix.Transpose(m), Is.EqualTo(Matrix.Create(0, 0)));
        }

        [Test]
        public void Transpose_1x1()
        {
            var m = Matrix.Create(1, 1,
                1.0);
            
            Assert.That(Matrix.Transpose(m), Is.EqualTo(Matrix.Create(1, 1,
                1.0)));
        }

        [Test]
        public void Transpose_1x2()
        {
            var m = Matrix.Create(1, 2,
                1.0, 2.0);
            
            Assert.That(Matrix.Transpose(m), Is.EqualTo(Matrix.Create(2, 1,
                1.0,
                2.0)));
        }

        [Test]
        public void Transpose_1x3()
        {
            var m = Matrix.Create(1, 3,
                1.0, 2.0, 3.0);
            
            Assert.That(Matrix.Transpose(m), Is.EqualTo(Matrix.Create(3, 1,
                1.0,
                2.0,
                3.0)));
        }

        [Test]
        public void Transpose_1x4()
        {
            var m = Matrix.Create(1, 4,
                1.0, 2.0, 3.0, 4.0);
            
            Assert.That(Matrix.Transpose(m), Is.EqualTo(Matrix.Create(4, 1,
                1.0,
                2.0,
                3.0,
                4.0)));
        }

        [Test]
        public void Transpose_2x1()
        {
            var m = Matrix.Create(2, 1,
                1.0,
                2.0);
            
            Assert.That(Matrix.Transpose(m), Is.EqualTo(Matrix.Create(1, 2,
                1.0, 2.0)));
        }

        [Test]
        public void Transpose_2x2()
        {
            var m = Matrix.Create(2, 2,
                1.0, 2.0,
                3.0, 4.0);
            
            Assert.That(Matrix.Transpose(m), Is.EqualTo(Matrix.Create(2, 2,
                1.0, 3.0,
                2.0, 4.0)));
        }

        [Test]
        public void Transpose_2x3()
        {
            var m = Matrix.Create(2, 3,
                1.0, 2.0, 3.0, 
                4.0, 5.0, 6.0);
            
            Assert.That(Matrix.Transpose(m), Is.EqualTo(Matrix.Create(3, 2,
                1.0, 4.0,
                2.0, 5.0,
                3.0, 6.0)));
        }

        [Test]
        public void Transpose_2x4()
        {
            var m = Matrix.Create(2, 4,
                1.0, 2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0, 8.0);
            
            Assert.That(Matrix.Transpose(m), Is.EqualTo(Matrix.Create(4, 2,
                1.0, 5.0,
                2.0, 6.0,
                3.0, 7.0,
                4.0, 8.0)));
        }

        [Test]
        public void Transpose_3x1()
        {
            var m = Matrix.Create(3, 1,
                1.0,
                2.0,
                3.0);
            
            Assert.That(Matrix.Transpose(m), Is.EqualTo(Matrix.Create(1, 3,
                1.0, 2.0, 3.0)));
        }

        [Test]
        public void Transpose_3x2()
        {
            var m = Matrix.Create(3, 2,
                1.0, 2.0,
                3.0, 4.0,
                5.0, 6.0);
            
            Assert.That(Matrix.Transpose(m), Is.EqualTo(Matrix.Create(2, 3,
                1.0, 3.0, 5.0,
                2.0, 4.0, 6.0)));
        }

        [Test]
        public void Transpose_3x3()
        {
            var m = Matrix.Create(3, 3,
                1.0, 2.0, 3.0, 
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0);
            
            Assert.That(Matrix.Transpose(m), Is.EqualTo(Matrix.Create(3, 3,
                1.0, 4.0, 7.0, 
                2.0, 5.0, 8.0,
                3.0, 6.0, 9.0)));
        }

        [Test]
        public void Transpose_3x4()
        {
            var m = Matrix.Create(3, 4,
                1.0, 2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0, 8.0, 
                9.0, 10.0, 11.0, 12.0);
            
            Assert.That(Matrix.Transpose(m), Is.EqualTo(Matrix.Create(4, 3,
                1.0, 5.0, 9.0, 
                2.0, 6.0, 10.0,
                3.0, 7.0, 11.0,
                4.0, 8.0, 12.0)));
        }

        [Test]
        public void Transpose_4x1()
        {
            var m = Matrix.Create(4, 1,
                1.0,
                2.0,
                3.0,
                4.0);
            
            Assert.That(Matrix.Transpose(m), Is.EqualTo(Matrix.Create(1, 4,
                1.0, 2.0, 3.0, 4.0)));
        }

        [Test]
        public void Transpose_4x2()
        {
            var m = Matrix.Create(4, 2,
                1.0, 2.0,
                3.0, 4.0,
                5.0, 6.0,
                7.0, 8.0);
            
            Assert.That(Matrix.Transpose(m), Is.EqualTo(Matrix.Create(2, 4,
                1.0, 3.0, 5.0, 7.0,
                2.0, 4.0, 6.0, 8.0)));
        }

        [Test]
        public void Transpose_4x3()
        {
            var m = Matrix.Create(4, 3,
                1.0, 2.0, 3.0, 
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0,
                10.0, 11.0, 12.0);
            
            Assert.That(Matrix.Transpose(m), Is.EqualTo(Matrix.Create(3, 4,
                1.0, 4.0, 7.0, 10.0,
                2.0, 5.0, 8.0, 11.0,
                3.0, 6.0, 9.0, 12.0)));
        }

        [Test]
        public void Transpose_4x4()
        {
            var m = Matrix.Create(4, 4,
                1.0, 2.0, 3.0, 4.0,
                5.0, 6.0, 7.0, 8.0,
                9.0, 10.0, 11.0, 12.0,
                13.0, 14.0, 15.0, 16.0);
            
            Assert.That(Matrix.Transpose(m), Is.EqualTo(Matrix.Create(4, 4,
                1.0, 5.0, 9.0, 13.0,
                2.0, 6.0, 10.0, 14.0,
                3.0, 7.0, 11.0, 15.0,
                4.0, 8.0, 12.0, 16.0)));
        }
        
        #endregion Transpose matrix
        
        #region -A (Negate matrix)
        
        [Test]
        public void Operator_Negate_ZeroMatrix(
            [Values(0, 1, 2, 3, 4)] int rows,
            [Values(0, 1, 2, 3, 4)] int cols)
        {
            var A = Matrix.ZeroMatrix(rows, cols);

            var zero = -A;
            
            Assert.That(zero, Is.EqualTo(A));
        }
        
        [Test]
        public void Operator_Negate_1x1()
        {
            var A = Matrix.Create(1, 1,
                1.0);
            
            Assert.That(-A, Is.EqualTo(Matrix.Create(1, 1,
                -1.0)));
        }
        
        [Test]
        public void Operator_Negate_1x2()
        {
            var A = Matrix.Create(1, 2,
                1.0, 2.0);
            
            Assert.That(-A, Is.EqualTo(Matrix.Create(1, 2,
                -1.0, -2.0)));
        }
        
        [Test]
        public void Operator_Negate_1x3()
        {
            var A = Matrix.Create(1, 3,
                1.0, 2.0, 3.0);
            
            Assert.That(-A, Is.EqualTo(Matrix.Create(1, 3,
                -1.0, -2.0, -3.0)));
        }
        
        [Test]
        public void Operator_Negate_1x4()
        {
            var A = Matrix.Create(1, 4,
                1.0, 2.0, 3.0, 4.0);
            
            Assert.That(-A, Is.EqualTo(Matrix.Create(1, 4,
                -1.0, -2.0, -3.0, -4.0)));
        }
        
        [Test]
        public void Operator_Negate_2x1()
        {
            var A = Matrix.Create(2, 1,
                1.0,
                2.0);
            
            Assert.That(-A, Is.EqualTo(Matrix.Create(2, 1,
                -1.0,
                -2.0)));
        }
        
        [Test]
        public void Operator_Negate_2x2()
        {
            var A = Matrix.Create(2, 2,
                1.0, 2.0,
                3.0, 4.0);
            
            Assert.That(-A, Is.EqualTo(Matrix.Create(2, 2,
                -1.0, -2.0,
                -3.0, -4.0)));
        }
        
        [Test]
        public void Operator_Negate_2x3()
        {
            var A = Matrix.Create(2, 3,
                1.0, 2.0, 3.0, 
                4.0, 5.0, 6.0);
            
            Assert.That(-A, Is.EqualTo(Matrix.Create(2, 3,
                -1.0, -2.0, -3.0, 
                -4.0, -5.0, -6.0)));
        }
        
        [Test]
        public void Operator_Negate_2x4()
        {
            var A = Matrix.Create(2, 4,
                1.0, 2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0, 8.0);
            
            Assert.That(-A, Is.EqualTo(Matrix.Create(2, 4,
                -1.0, -2.0, -3.0, -4.0, 
                -5.0, -6.0, -7.0, -8.0)));
        }
        
        [Test]
        public void Operator_Negate_3x1()
        {
            var A = Matrix.Create(3, 1,
                1.0,
                2.0,
                3.0);
            
            Assert.That(-A, Is.EqualTo(Matrix.Create(3, 1,
                -1.0,
                -2.0,
                -3.0)));
        }
        
        [Test]
        public void Operator_Negate_3x2()
        {
            var A = Matrix.Create(3, 2,
                1.0, 2.0,
                3.0, 4.0,
                5.0, 6.0);
            
            Assert.That(-A, Is.EqualTo(Matrix.Create(3, 2,
                -1.0, -2.0,
                -3.0, -4.0,
                -5.0, -6.0)));
        }
        
        [Test]
        public void Operator_Negate_3x3()
        {
            var A = Matrix.Create(3, 3,
                1.0, 2.0, 3.0, 
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0);
            
            Assert.That(-A, Is.EqualTo(Matrix.Create(3, 3,
                -1.0, -2.0, -3.0, 
                -4.0, -5.0, -6.0,
                -7.0, -8.0, -9.0)));
        }
        
        [Test]
        public void Operator_Negate_3x4()
        {
            var A = Matrix.Create(3, 4,
                1.0, 2.0, 3.0, 4.0,
                5.0, 6.0, 7.0, 8.0, 
                9.0, 10.0, 11.0, 12.0);
            
            Assert.That(-A, Is.EqualTo(Matrix.Create(3, 4,
                -1.0, -2.0, -3.0, -4.0,
                -5.0, -6.0, -7.0, -8.0, 
                -9.0, -10.0, -11.0, -12.0)));
        }
        
        [Test]
        public void Operator_Negate_4x1()
        {
            var A = Matrix.Create(4, 1,
                1.0,
                2.0,
                3.0,
                4.0);
            
            Assert.That(-A, Is.EqualTo(Matrix.Create(4, 1,
                -1.0,
                -2.0,
                -3.0,
                -4.0)));
        }
        
        [Test]
        public void Operator_Negate_4x2()
        {
            var A = Matrix.Create(4, 2,
                1.0, 2.0,
                3.0, 4.0,
                5.0, 6.0,
                7.0, 8.0);
            
            Assert.That(-A, Is.EqualTo(Matrix.Create(4, 2,
                -1.0, -2.0,
                -3.0, -4.0,
                -5.0, -6.0,
                -7.0, -8.0)));
        }
        
        [Test]
        public void Operator_Negate_4x3()
        {
            var A = Matrix.Create(4, 3,
                1.0, 2.0, 3.0, 
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0,
                10.0, 11.0, 12.0);
            
            Assert.That(-A, Is.EqualTo(Matrix.Create(4, 3,
                -1.0, -2.0, -3.0, 
                -4.0, -5.0, -6.0,
                -7.0, -8.0, -9.0,
                -10.0, -11.0, -12.0)));
        }
        
        [Test]
        public void Operator_Negate_4x4()
        {
            var A = Matrix.Create(4, 4,
                1.0, 2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0, 8.0, 
                9.0, 10.0, 11.0, 12.0,
                13.0, 14.0, 15.0, 16.0);
            
            Assert.That(-A, Is.EqualTo(Matrix.Create(4, 4,
                -1.0, -2.0, -3.0, -4.0, 
                -5.0, -6.0, -7.0, -8.0, 
                -9.0, -10.0, -11.0, -12.0,
                -13.0, -14.0, -15.0, -16.0)));
        }

        #endregion -A (Negate matrix)
        
        #region A + B (Sum two matrices)

        [Test]
        public void Operator_Plus_Throws_With_DifferentDimmensions(
            [Values(0, 1, 2, 3, 4)] int rows,
            [Values(0, 1, 2, 3, 4)] int cols)
        {
            {
                var A = Matrix.Create(rows, cols);
                var B = Matrix.Create(rows, cols+1);

                Assert.Throws<MException>(() => { var res = A + B; });
            }
            {
                var A = Matrix.Create(rows, cols);
                var B = Matrix.Create(rows+1, cols);

                Assert.Throws<MException>(() => { var res = A + B; });
            }
        }
        
        [Test]
        public void Operator_1x1_Plus_1x1()
        {
            var A = Matrix.Create(1, 1,
                1.0);
            var B = Matrix.Create(1, 1,
                100.0);

            var res = A + B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(1, 1,
                101.0)));
        }
        
        [Test]
        public void Operator_1x2_Plus_1x2()
        {
            var A = Matrix.Create(1, 2,
                1.0, 2.0);
            var B = Matrix.Create(1, 2,
                100.0, 200.0);

            var res = A + B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(1, 2,
                101.0, 202.0)));
        }
        
        [Test]
        public void Operator_1x3_Plus_1x3()
        {
            var A = Matrix.Create(1, 3,
                1.0, 2.0, 3.0);
            var B = Matrix.Create(1, 3,
                100.0, 200.0, 300.0);

            var res = A + B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(1, 3,
                101.0, 202.0, 303.0)));
        }
        
        [Test]
        public void Operator_1x4_Plus_1x4()
        {
            var A = Matrix.Create(1, 4,
                1.0, 2.0, 3.0, 4.0);
            var B = Matrix.Create(1, 4,
                100.0, 200.0, 300.0, 400.0);

            var res = A + B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(1, 4,
                101.0, 202.0, 303.0, 404.0)));
        }
        
        [Test]
        public void Operator_2x1_Plus_2x1()
        {
            var A = Matrix.Create(2, 1,
                1.0,
                2.0);
            var B = Matrix.Create(2, 1,
                100.0,
                200.0);

            var res = A + B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(2, 1,
                101.0,
                202.0)));
        }
        
        [Test]
        public void Operator_2x2_Plus_2x2()
        {
            var A = Matrix.Create(2, 2,
                1.0, 2.0,
                3.0, 4.0);
            var B = Matrix.Create(2, 2,
                100.0, 200.0,
                300.0, 400.0);

            var res = A + B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(2, 2,
                101.0, 202.0,
                303.0, 404.0)));
        }
        
        [Test]
        public void Operator_2x3_Plus_2x3()
        {
            var A = Matrix.Create(2, 3,
                1.0, 2.0, 3.0, 
                4.0, 5.0, 6.0);
            var B = Matrix.Create(2, 3,
                100.0, 200.0, 300.0, 
                400.0, 500.0, 600.0);

            var res = A + B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(2, 3,
                101.0, 202.0, 303.0, 
                404.0, 505.0, 606.0)));
        }
        
        [Test]
        public void Operator_2x4_Plus_2x4()
        {
            var A = Matrix.Create(2, 4,
                1.0, 2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0, 8.0);
            var B = Matrix.Create(2, 4,
                100.0, 200.0, 300.0, 400.0,
                500.0, 600.0, 700.0, 800.0);

            var res = A + B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(2, 4,
                101.0, 202.0, 303.0, 404.0,
                505.0, 606.0, 707.0, 808.0)));
        }
        
        [Test]
        public void Operator_3x1_Plus_3x1()
        {
            var A = Matrix.Create(3, 1,
                1.0,
                2.0,
                3.0);
            var B = Matrix.Create(3, 1,
                100.0,
                200.0,
                300.0);

            var res = A + B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(3, 1,
                101.0,
                202.0,
                303.0)));
        }
        
        [Test]
        public void Operator_3x2_Plus_3x2()
        {
            var A = Matrix.Create(3, 2,
                1.0, 2.0,
                3.0, 4.0,
                5.0, 6.0);
            var B = Matrix.Create(3, 2,
                100.0, 200.0,
                300.0, 400.0,
                500.0, 600.0);

            var res = A + B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(3, 2,
                101.0, 202.0,
                303.0, 404.0,
                505.0, 606.0)));
        }
        
        [Test]
        public void Operator_3x3_Plus_3x3()
        {
            var A = Matrix.Create(3, 3,
                1.0, 2.0, 3.0, 
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0);
            var B = Matrix.Create(3, 3,
                100.0, 200.0, 300.0,
                400.0, 500.0, 600.0,
                700.0, 800.0, 900.0);

            var res = A + B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(3, 3,
                101.0, 202.0, 303.0,
                404.0, 505.0, 606.0,
                707.0, 808.0, 909.0)));
        }
        
        [Test]
        public void Operator_3x4_Plus_3x4()
        {
            var A = Matrix.Create(3, 4,
                1.0, 2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0, 8.0, 
                9.0, 10.0, 11.0, 12.0);
            var B = Matrix.Create(3, 4,
                100.0, 200.0, 300.0, 400.0, 
                500.0, 600.0, 700.0, 800.0,
                900.0, 1000.0, 1100.0, 1200.0);

            var res = A + B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(3, 4,
                101.0, 202.0, 303.0, 404.0, 
                505.0, 606.0, 707.0, 808.0,
                909.0, 1010.0, 1111.0, 1212.0)));
        }

        [Test]
        public void Operator_4x1_Plus_4x1()
        {
            var A = Matrix.Create(4, 1,
                1.0,
                2.0,
                3.0,
                4.0);
            var B = Matrix.Create(4, 1,
                100.0,
                200.0,
                300.0,
                400.0);

            var res = A + B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(4, 1,
                101.0,
                202.0,
                303.0,
                404.0)));
        }

        [Test]
        public void Operator_4x2_Plus_4x2()
        {
            var A = Matrix.Create(4, 2,
                1.0, 2.0,
                3.0, 4.0,
                5.0, 6.0,
                7.0, 8.0);
            var B = Matrix.Create(4, 2,
                100.0, 200.0,
                300.0, 400.0,
                500.0, 600.0,
                700.0, 800.0);

            var res = A + B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(4, 2,
                101.0, 202.0,
                303.0, 404.0,
                505.0, 606.0,
                707.0, 808.0)));
        }

        [Test]
        public void Operator_4x3_Plus_4x3()
        {
            var A = Matrix.Create(4, 3,
                1.0, 2.0, 3.0, 
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0,
                10.0, 11.0, 12.0);
            var B = Matrix.Create(4, 3,
                100.0, 200.0, 300.0, 
                400.0, 500.0, 600.0,
                700.0, 800.0, 900.0,
                1000.0, 1100.0, 1200.0);

            var res = A + B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(4, 3,
                101.0, 202.0, 303.0, 
                404.0, 505.0, 606.0,
                707.0, 808.0, 909.0,
                1010.0, 1111.0, 1212.0)));
        }

        [Test]
        public void Operator_4x4_Plus_4x4()
        {
            var A = Matrix.Create(4, 4,
                1.0, 2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0, 8.0, 
                9.0, 10.0, 11.0, 12.0,
                13.0, 14.0, 15.0, 16.0);
            var B = Matrix.Create(4, 4,
                100.0, 200.0, 300.0, 400.0,
                500.0, 600.0, 700.0, 800.0,
                900.0, 1000.0, 1100.0, 1200.0,
                1300.0, 1400.0, 1500.0, 1600.0);

            var res = A + B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(4, 4,
                101.0, 202.0, 303.0, 404.0,
                505.0, 606.0, 707.0, 808.0,
                909.0, 1010.0, 1111.0, 1212.0,
                1313.0, 1414.0, 1515.0, 1616.0)));
        }
        
        #endregion A + B (Sum two matrices)
        
        #region A - B (Subtract one matrix from another)
        
        [Test]
        public void Operator_Minus_Throws_With_DifferentDimmensions(
            [Values(0, 1, 2, 3, 4)] int rows,
            [Values(0, 1, 2, 3, 4)] int cols)
        {
            {
                var A = Matrix.Create(rows, cols);
                var B = Matrix.Create(rows, cols+1);

                Assert.Throws<MException>(() => { var res = A - B; });
            }
            {
                var A = Matrix.Create(rows, cols);
                var B = Matrix.Create(rows+1, cols);

                Assert.Throws<MException>(() => { var res = A - B; });
            }
        }
        
        [Test]
        public void Operator_1x1_Minus_1x1()
        {
            var A = Matrix.Create(1, 1,
                1.0);
            var B = Matrix.Create(1, 1,
                100.0);

            var res = A - B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(1, 1,
                -99.0)));
        }
        
        [Test]
        public void Operator_1x2_Minus_1x2()
        {
            var A = Matrix.Create(1, 2,
                1.0, 2.0);
            var B = Matrix.Create(1, 2,
                100.0, 200.0);

            var res = A - B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(1, 2,
                -99.0, -198.0)));
        }
        
        [Test]
        public void Operator_1x3_Minus_1x3()
        {
            var A = Matrix.Create(1, 3,
                1.0, 2.0, 3.0);
            var B = Matrix.Create(1, 3,
                100.0, 200.0, 300.0);

            var res = A - B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(1, 3,
                -99.0, -198.0, -297.0)));
        }
        
        [Test]
        public void Operator_1x4_Minus_1x4()
        {
            var A = Matrix.Create(1, 4,
                1.0, 2.0, 3.0, 4.0);
            var B = Matrix.Create(1, 4,
                100.0, 200.0, 300.0, 400.0);

            var res = A - B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(1, 4,
                -99.0, -198.0, -297.0, -396.0)));
        }
        
        [Test]
        public void Operator_2x1_Minus_2x1()
        {
            var A = Matrix.Create(2, 1,
                1.0,
                2.0);
            var B = Matrix.Create(2, 1,
                100.0,
                200.0);

            var res = A - B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(2, 1,
                -99.0,
                -198.0)));
        }
        
        [Test]
        public void Operator_2x2_Minus_2x2()
        {
            var A = Matrix.Create(2, 2,
                1.0, 2.0,
                3.0, 4.0);
            var B = Matrix.Create(2, 2,
                100.0, 200.0,
                300.0, 400.0);

            var res = A - B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(2, 2,
                -99.0, -198.0,
                -297.0, -396.0)));
        }
        
        [Test]
        public void Operator_2x3_Minus_2x3()
        {
            var A = Matrix.Create(2, 3,
                1.0, 2.0, 3.0, 
                4.0, 5.0, 6.0);
            var B = Matrix.Create(2, 3,
                100.0, 200.0, 300.0, 
                400.0, 500.0, 600.0);

            var res = A - B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(2, 3,
                -99.0, -198.0, -297.0, 
                -396.0, -495.0, -594.0)));
        }
        
        [Test]
        public void Operator_2x4_Minus_2x4()
        {
            var A = Matrix.Create(2, 4,
                1.0, 2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0, 8.0);
            var B = Matrix.Create(2, 4,
                100.0, 200.0, 300.0, 400.0,
                500.0, 600.0, 700.0, 800.0);

            var res = A - B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(2, 4,
                -99.0, -198.0, -297.0, -396.0,
                -495.0, -594.0, -693.0, -792.0)));
        }
        
        [Test]
        public void Operator_3x1_Minus_3x1()
        {
            var A = Matrix.Create(3, 1,
                1.0,
                2.0,
                3.0);
            var B = Matrix.Create(3, 1,
                100.0,
                200.0,
                300.0);

            var res = A - B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(3, 1,
                -99.0,
                -198.0,
                -297.0)));
        }
        
        [Test]
        public void Operator_3x2_Minus_3x2()
        {
            var A = Matrix.Create(3, 2,
                1.0, 2.0,
                3.0, 4.0,
                5.0, 6.0);
            var B = Matrix.Create(3, 2,
                100.0, 200.0,
                300.0, 400.0,
                500.0, 600.0);

            var res = A - B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(3, 2,
                -99.0, -198.0,
                -297.0, -396.0,
                -495.0, -594.0)));
        }
        
        [Test]
        public void Operator_3x3_Minus_3x3()
        {
            var A = Matrix.Create(3, 3,
                1.0, 2.0, 3.0, 
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0);
            var B = Matrix.Create(3, 3,
                100.0, 200.0, 300.0,
                400.0, 500.0, 600.0,
                700.0, 800.0, 900.0);

            var res = A - B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(3, 3,
                -99.0, -198.0, -297.0,
                -396.0, -495.0, -594.0,
                -693.0, -792.0, -891.0)));
        }
        
        [Test]
        public void Operator_3x4_Minus_3x4()
        {
            var A = Matrix.Create(3, 4,
                1.0, 2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0, 8.0, 
                9.0, 10.0, 11.0, 12.0);
            var B = Matrix.Create(3, 4,
                100.0, 200.0, 300.0, 400.0, 
                500.0, 600.0, 700.0, 800.0,
                900.0, 1000.0, 1100.0, 1200.0);

            var res = A - B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(3, 4,
                -99.0, -198.0, -297.0, -396.0, 
                -495.0, -594.0, -693.0, -792.0,
                -891.0, -990.0, -1089.0, -1188.0)));
        }

        [Test]
        public void Operator_4x1_Minus_4x1()
        {
            var A = Matrix.Create(4, 1,
                1.0,
                2.0,
                3.0,
                4.0);
            var B = Matrix.Create(4, 1,
                100.0,
                200.0,
                300.0,
                400.0);

            var res = A - B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(4, 1,
                -99.0,
                -198.0,
                -297.0,
                -396.0)));
        }

        [Test]
        public void Operator_4x2_Minus_4x2()
        {
            var A = Matrix.Create(4, 2,
                1.0, 2.0,
                3.0, 4.0,
                5.0, 6.0,
                7.0, 8.0);
            var B = Matrix.Create(4, 2,
                100.0, 200.0,
                300.0, 400.0,
                500.0, 600.0,
                700.0, 800.0);

            var res = A - B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(4, 2,
                -99.0, -198.0,
                -297.0, -396.0,
                -495.0, -594.0,
                -693.0, -792.0)));
        }

        [Test]
        public void Operator_4x3_Minus_4x3()
        {
            var A = Matrix.Create(4, 3,
                1.0, 2.0, 3.0, 
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0,
                10.0, 11.0, 12.0);
            var B = Matrix.Create(4, 3,
                100.0, 200.0, 300.0, 
                400.0, 500.0, 600.0,
                700.0, 800.0, 900.0,
                1000.0, 1100.0, 1200.0);

            var res = A - B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(4, 3,
                -99.0, -198.0, -297.0, 
                -396.0, -495.0, -594.0,
                -693.0, -792.0, -891.0,
                -990.0, -1089.0, -1188.0)));
        }

        [Test]
        public void Operator_4x4_Minus_4x4()
        {
            var A = Matrix.Create(4, 4,
                1.0, 2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0, 8.0, 
                9.0, 10.0, 11.0, 12.0,
                13.0, 14.0, 15.0, 16.0);
            var B = Matrix.Create(4, 4,
                100.0, 200.0, 300.0, 400.0,
                500.0, 600.0, 700.0, 800.0,
                900.0, 1000.0, 1100.0, 1200.0,
                1300.0, 1400.0, 1500.0, 1600.0);

            var res = A - B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(4, 4,
                -99.0, -198.0, -297.0, -396.0, 
                -495.0, -594.0, -693.0, -792.0,
                -891.0, -990.0, -1089.0, -1188.0,
                -1287.0, -1386.0, -1485.0, -1584.0)));
        }

        #endregion A - B (Subtract one matrix from another)
        
        #region x * A (Multiply number with Matrix)
        
        [Test]
        public void Operator_x_Multiply_1x1()
        {
            var A = Matrix.Create(1, 1,
                1.0);
            
            Assert.That(-2 * A, Is.EqualTo(Matrix.Create(1, 1,
                -2.0)));
            
            Assert.That(-1 * A, Is.EqualTo(Matrix.Create(1, 1,
                -1.0)));
            
            Assert.That(0 * A, Is.EqualTo(Matrix.ZeroMatrix(1, 1)));
            
            Assert.That(1 * A, Is.EqualTo(Matrix.Create(1, 1,
                1.0)));
            
            Assert.That(2 * A, Is.EqualTo(Matrix.Create(1, 1,
                2.0)));
        }
        
        [Test]
        public void Operator_x_Multiply_1x2()
        {
            var A = Matrix.Create(1, 2,
                1.0, 2.0);
            
            Assert.That(-2 * A, Is.EqualTo(Matrix.Create(1, 2,
                
                -2.0, -4.0)));
            Assert.That(-1 * A, Is.EqualTo(Matrix.Create(1, 2,
                -1.0, -2.0)));
            
            Assert.That(0 * A, Is.EqualTo(Matrix.ZeroMatrix(1, 2)));
            
            Assert.That(1 * A, Is.EqualTo(Matrix.Create(1, 2,
                1.0, 2.0)));
            
            Assert.That(2 * A, Is.EqualTo(Matrix.Create(1, 2,
                2.0, 4.0)));
        }
        
        [Test]
        public void Operator_x_Multiply_1x3()
        {
            var A = Matrix.Create(1, 3,
                1.0, 2.0, 3.0);
            
            Assert.That(-2 * A, Is.EqualTo(Matrix.Create(1, 3,
                -2.0, -4.0, -6.0)));
            
            Assert.That(-1 * A, Is.EqualTo(Matrix.Create(1, 3,
                -1.0, -2.0, -3.0)));
            
            Assert.That(0 * A, Is.EqualTo(Matrix.ZeroMatrix(1, 3)));
            
            Assert.That(1 * A, Is.EqualTo(Matrix.Create(1, 3,
                1.0, 2.0, 3.0)));
            
            Assert.That(2 * A, Is.EqualTo(Matrix.Create(1, 3,
                2.0, 4.0, 6.0)));
        }
        
        [Test]
        public void Operator_x_Multiply_1x4()
        {
            var A = Matrix.Create(1, 4,
                1.0, 2.0, 3.0, 4.0);
            
            Assert.That(-2 * A, Is.EqualTo(Matrix.Create(1, 4,
                -2.0, -4.0, -6.0, -8.0)));
            
            Assert.That(-1 * A, Is.EqualTo(Matrix.Create(1, 4,
                -1.0, -2.0, -3.0, -4.0)));
            
            Assert.That(0 * A, Is.EqualTo(Matrix.ZeroMatrix(1, 4)));
            
            Assert.That(1 * A, Is.EqualTo(Matrix.Create(1, 4,
                1.0, 2.0, 3.0, 4.0)));
            
            Assert.That(2 * A, Is.EqualTo(Matrix.Create(1, 4,
                2.0, 4.0, 6.0, 8.0)));
        }
        
        [Test]
        public void Operator_x_Multiply_2x1()
        {
            var A = Matrix.Create(2, 1,
                1.0,
                2.0);
            
            Assert.That(-2 * A, Is.EqualTo(Matrix.Create(2, 1,
                -2.0,
                -4.0)));
            
            Assert.That(-1 * A, Is.EqualTo(Matrix.Create(2, 1,
                -1.0,
                -2.0)));
            
            Assert.That(0 * A, Is.EqualTo(Matrix.ZeroMatrix(2, 1)));
            
            Assert.That(1 * A, Is.EqualTo(Matrix.Create(2, 1,
                1.0,
                2.0)));
            
            Assert.That(2 * A, Is.EqualTo(Matrix.Create(2, 1,
                2.0,
                4.0)));
        }
        
        [Test]
        public void Operator_x_Multiply_2x2()
        {
            var A = Matrix.Create(2, 2,
                1.0, 2.0,
                3.0, 4.0);
            
            Assert.That(-2 * A, Is.EqualTo(Matrix.Create(2, 2,
                -2.0, -4.0,
                -6.0, -8.0)));
            
            Assert.That(-1 * A, Is.EqualTo(Matrix.Create(2, 2,
                -1.0, -2.0,
                -3.0, -4.0)));
            
            Assert.That(0 * A, Is.EqualTo(Matrix.ZeroMatrix(2, 2)));
            
            Assert.That(1 * A, Is.EqualTo(Matrix.Create(2, 2,
                1.0, 2.0,
                3.0, 4.0)));
            
            Assert.That(2 * A, Is.EqualTo(Matrix.Create(2, 2,
                2.0, 4.0,
                6.0, 8.0)));
        }
        
        [Test]
        public void Operator_x_Multiply_2x3()
        {
            var A = Matrix.Create(2, 3,
                1.0, 2.0, 3.0, 
                4.0, 5.0, 6.0);
            
            Assert.That(-1 * A, Is.EqualTo(Matrix.Create(2, 3,
                -1.0, -2.0, -3.0, 
                -4.0, -5.0, -6.0)));
            
            Assert.That(-2 * A, Is.EqualTo(Matrix.Create(2, 3,
                -2.0, -4.0, -6.0, 
                -8.0, -10.0, -12.0)));
            
            Assert.That(0 * A, Is.EqualTo(Matrix.ZeroMatrix(2, 3)));
            
            Assert.That(1 * A, Is.EqualTo(Matrix.Create(2, 3,
                1.0, 2.0, 3.0, 
                4.0, 5.0, 6.0)));
            
            Assert.That(2 * A, Is.EqualTo(Matrix.Create(2, 3,
                2.0, 4.0, 6.0, 
                8.0, 10.0, 12.0)));
        }
        
        [Test]
        public void Operator_x_Multiply_2x4()
        {
            var A = Matrix.Create(2, 4,
                1.0, 2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0, 8.0);
            
            Assert.That(-2 * A, Is.EqualTo(Matrix.Create(2, 4,
                -2.0, -4.0, -6.0, -8.0, 
                -10.0, -12.0, -14.0, -16.0)));
            
            Assert.That(-1 * A, Is.EqualTo(Matrix.Create(2, 4,
                -1.0, -2.0, -3.0, -4.0, 
                -5.0, -6.0, -7.0, -8.0)));
            
            Assert.That(0 * A, Is.EqualTo(Matrix.ZeroMatrix(2, 4)));
            
            Assert.That(1 * A, Is.EqualTo(Matrix.Create(2, 4,
                1.0, 2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0, 8.0)));
            
            Assert.That(2 * A, Is.EqualTo(Matrix.Create(2, 4,
                2.0, 4.0, 6.0, 8.0, 
                10.0, 12.0, 14.0, 16.0)));
        }
        
        [Test]
        public void Operator_x_Multiply_3x1()
        {
            var A = Matrix.Create(3, 1,
                1.0,
                2.0,
                3.0);
            
            Assert.That(-2 * A, Is.EqualTo(Matrix.Create(3, 1,
                -2.0,
                -4.0,
                -6.0)));
            
            Assert.That(-1 * A, Is.EqualTo(Matrix.Create(3, 1,
                -1.0,
                -2.0,
                -3.0)));
            
            Assert.That(0 * A, Is.EqualTo(Matrix.ZeroMatrix(3, 1)));
            
            Assert.That(1 * A, Is.EqualTo(Matrix.Create(3, 1,
                1.0,
                2.0,
                3.0)));
            
            Assert.That(2 * A, Is.EqualTo(Matrix.Create(3, 1,
                2.0,
                4.0,
                6.0)));
        }
        
        [Test]
        public void Operator_x_Multiply_3x2()
        {
            var A = Matrix.Create(3, 2,
                1.0, 2.0,
                3.0, 4.0,
                5.0, 6.0);
            
            Assert.That(-2 * A, Is.EqualTo(Matrix.Create(3, 2,
                -2.0, -4.0,
                -6.0, -8.0,
                -10.0, -12.0)));
            
            Assert.That(-1 * A, Is.EqualTo(Matrix.Create(3, 2,
                -1.0, -2.0,
                -3.0, -4.0,
                -5.0, -6.0)));
            
            Assert.That(0 * A, Is.EqualTo(Matrix.ZeroMatrix(3, 2)));
            
            Assert.That(1 * A, Is.EqualTo(Matrix.Create(3, 2,
                1.0, 2.0,
                3.0, 4.0,
                5.0, 6.0)));
            
            Assert.That(2 * A, Is.EqualTo(Matrix.Create(3, 2,
                2.0, 4.0,
                6.0, 8.0,
                10.0, 12.0)));
        }
        
        [Test]
        public void Operator_x_Multiply_3x3()
        {
            var A = Matrix.Create(3, 3,
                1.0, 2.0, 3.0, 
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0);
            
            Assert.That(-2 * A, Is.EqualTo(Matrix.Create(3, 3,
                -2.0, -4.0, -6.0, 
                -8.0, -10.0, -12.0,
                -14.0, -16.0, -18.0)));
            
            Assert.That(-1 * A, Is.EqualTo(Matrix.Create(3, 3,
                -1.0, -2.0, -3.0, 
                -4.0, -5.0, -6.0,
                -7.0, -8.0, -9.0)));
            
            Assert.That(0 * A, Is.EqualTo(Matrix.ZeroMatrix(3, 3)));
            
            Assert.That(1 * A, Is.EqualTo(Matrix.Create(3, 3,
                1.0, 2.0, 3.0, 
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0)));
            
            Assert.That(2 * A, Is.EqualTo(Matrix.Create(3, 3,
                2.0, 4.0, 6.0, 
                8.0, 10.0, 12.0,
                14.0, 16.0, 18.0)));
        }
        
        [Test]
        public void Operator_x_Multiply_3x4()
        {
            var A = Matrix.Create(3, 4,
                1.0, 2.0, 3.0, 4.0,
                5.0, 6.0, 7.0, 8.0, 
                9.0, 10.0, 11.0, 12.0);
            
            Assert.That(-2 * A, Is.EqualTo(Matrix.Create(3, 4,
                -2.0, -4.0, -6.0, -8.0,
                -10.0, -12.0, -14.0, -16.0, 
                -18.0, -20.0, -22.0, -24.0)));
            
            Assert.That(-1 * A, Is.EqualTo(Matrix.Create(3, 4,
                -1.0, -2.0, -3.0, -4.0,
                -5.0, -6.0, -7.0, -8.0, 
                -9.0, -10.0, -11.0, -12.0)));
            
            Assert.That(0 * A, Is.EqualTo(Matrix.ZeroMatrix(3, 4)));
            
            Assert.That(1 * A, Is.EqualTo(Matrix.Create(3, 4,
                1.0, 2.0, 3.0, 4.0,
                5.0, 6.0, 7.0, 8.0, 
                9.0, 10.0, 11.0, 12.0)));
            
            Assert.That(2 * A, Is.EqualTo(Matrix.Create(3, 4,
                2.0, 4.0, 6.0, 8.0,
                10.0, 12.0, 14.0, 16.0, 
                18.0, 20.0, 22.0, 24.0)));
        }
        
        [Test]
        public void Operator_x_Multiply_4x1()
        {
            var A = Matrix.Create(4, 1,
                1.0,
                2.0,
                3.0,
                4.0);
            
            Assert.That(-2 * A, Is.EqualTo(Matrix.Create(4, 1,
                -2.0,
                -4.0,
                -6.0,
                -8.0)));
            
            Assert.That(-1 * A, Is.EqualTo(Matrix.Create(4, 1,
                -1.0,
                -2.0,
                -3.0,
                -4.0)));
            
            Assert.That(0 * A, Is.EqualTo(Matrix.ZeroMatrix(4, 1)));
            
            Assert.That(1 * A, Is.EqualTo(Matrix.Create(4, 1,
                1.0,
                2.0,
                3.0,
                4.0)));
            
            Assert.That(2 * A, Is.EqualTo(Matrix.Create(4, 1,
                2.0,
                4.0,
                6.0,
                8.0)));
        }
        
        [Test]
        public void Operator_x_Multiply_4x2()
        {
            var A = Matrix.Create(4, 2,
                1.0, 2.0,
                3.0, 4.0,
                5.0, 6.0,
                7.0, 8.0);
            
            Assert.That(-2 * A, Is.EqualTo(Matrix.Create(4, 2,
                -2.0, -4.0,
                -6.0, -8.0,
                -10.0, -12.0,
                -14.0, -16.0)));
            
            Assert.That(-1 * A, Is.EqualTo(Matrix.Create(4, 2,
                -1.0, -2.0,
                -3.0, -4.0,
                -5.0, -6.0,
                -7.0, -8.0)));
            
            Assert.That(0 * A, Is.EqualTo(Matrix.ZeroMatrix(4, 2)));
            
            Assert.That(1 * A, Is.EqualTo(Matrix.Create(4, 2,
                1.0, 2.0,
                3.0, 4.0,
                5.0, 6.0,
                7.0, 8.0)));
            
            Assert.That(2 * A, Is.EqualTo(Matrix.Create(4, 2,
                2.0, 4.0,
                6.0, 8.0,
                10.0, 12.0,
                14.0, 16.0)));
        }
        
        [Test]
        public void Operator_x_Multiply_4x3()
        {
            var A = Matrix.Create(4, 3,
                1.0, 2.0, 3.0, 
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0,
                10.0, 11.0, 12.0);
            
            Assert.That(-2 * A, Is.EqualTo(Matrix.Create(4, 3,
                -2.0, -4.0, -6.0, 
                -8.0, -10.0, -12.0,
                -14.0, -16.0, -18.0,
                -20.0, -22.0, -24.0)));
            
            Assert.That(-1 * A, Is.EqualTo(Matrix.Create(4, 3,
                -1.0, -2.0, -3.0, 
                -4.0, -5.0, -6.0,
                -7.0, -8.0, -9.0,
                -10.0, -11.0, -12.0)));
            
            Assert.That(0 * A, Is.EqualTo(Matrix.ZeroMatrix(4, 3)));
            
            Assert.That(1 * A, Is.EqualTo(Matrix.Create(4, 3,
                1.0, 2.0, 3.0, 
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0,
                10.0, 11.0, 12.0)));
            
            Assert.That(2 * A, Is.EqualTo(Matrix.Create(4, 3,
                2.0, 4.0, 6.0, 
                8.0, 10.0, 12.0,
                14.0, 16.0, 18.0,
                20.0, 22.0, 24.0)));
        }
        
        [Test]
        public void Operator_x_Multiply_4x4()
        {
            var A = Matrix.Create(4, 4,
                1.0, 2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0, 8.0, 
                9.0, 10.0, 11.0, 12.0,
                13.0, 14.0, 15.0, 16.0);
            
            Assert.That(-2 * A, Is.EqualTo(Matrix.Create(4, 4,
                -2.0, -4.0, -6.0, -8.0, 
                -10.0, -12.0, -14.0, -16.0, 
                -18.0, -20.0, -22.0, -24.0,
                -26.0, -28.0, -30.0, -32.0)));
            
            Assert.That(-1 * A, Is.EqualTo(Matrix.Create(4, 4,
                -1.0, -2.0, -3.0, -4.0, 
                -5.0, -6.0, -7.0, -8.0, 
                -9.0, -10.0, -11.0, -12.0,
                -13.0, -14.0, -15.0, -16.0)));
            
            Assert.That(0 * A, Is.EqualTo(Matrix.ZeroMatrix(4, 4)));
            
            Assert.That(1 * A, Is.EqualTo(Matrix.Create(4, 4,
                1.0, 2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0, 8.0, 
                9.0, 10.0, 11.0, 12.0,
                13.0, 14.0, 15.0, 16.0)));
            
            Assert.That(2 * A, Is.EqualTo(Matrix.Create(4, 4,
                2.0, 4.0, 6.0, 8.0, 
                10.0, 12.0, 14.0, 16.0, 
                18.0, 20.0, 22.0, 24.0,
                26.0, 28.0, 30.0, 32.0)));
        }
        
        #endregion x * A (Multiply number with Matrix)
        
        #region A * B (Multiply two matrices)
        
        [Test]
        public void Operator_Multiply_DoesNotThrow(
            [Values(0, 1, 2, 3, 4)] int rowsA,
            [Values(0, 1, 2, 3, 4)] int colsA)
        {
            var A = Matrix.Create(rowsA, colsA);
            var B = Matrix.Create(colsA, rowsA);
            
            var res = A * B;
            Assert.That(res.rows, Is.EqualTo(A.rows));
            Assert.That(res.cols, Is.EqualTo(B.cols));
        }
        
        [Test]
        public void Operator_Multiply_Throws_When_ColsOfA_NotEqual_To_RowsOfB(
            [Values(0, 1, 2, 3, 4)] int anyRows,
            [Values(0, 1, 2, 3, 4)] int anyCols)
        {
            var createA = (int rows, int cols) => Matrix.Create(rows, cols);
            var createB = (int rows, int cols) => Matrix.Create(rows, cols);
            
            Assert.Throws<MException>(() => { var res = createA(anyRows, 3) * createB(4, anyCols); });
            
            Assert.Throws<MException>(() => { var res = createA(anyRows, 2) * createB(4, anyCols); });
            Assert.Throws<MException>(() => { var res = createA(anyRows, 2) * createB(3, anyCols); });
            
            Assert.Throws<MException>(() => { var res = createA(anyRows, 1) * createB(4, anyCols); });
            Assert.Throws<MException>(() => { var res = createA(anyRows, 1) * createB(3, anyCols); });
            Assert.Throws<MException>(() => { var res = createA(anyRows, 1) * createB(2, anyCols); });
            
            Assert.Throws<MException>(() => { var res = createA(anyRows, 2) * createB(1, anyCols); });
            Assert.Throws<MException>(() => { var res = createA(anyRows, 3) * createB(1, anyCols); });
            Assert.Throws<MException>(() => { var res = createA(anyRows, 4) * createB(1, anyCols); });
            
            Assert.Throws<MException>(() => { var res = createA(anyRows, 3) * createB(2, anyCols); });
            Assert.Throws<MException>(() => { var res = createA(anyRows, 4) * createB(2, anyCols); });
            
            Assert.Throws<MException>(() => { var res = createA(anyRows, 4) * createB(3, anyCols); });
        }
        
        #region A * B ===> (1x1 * 1xN)
        // 1x1 * 1x1
        // 1x1 * 1x2
        // 1x1 * 1x3
        // 1x1 * 1x4
        
        [Test]
        public void Operator_1x1_Multiply_1x1()
        {
            var A = Matrix.Create(1, 1,
                2.0);
            var B = Matrix.Create(1, 1,
                100.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(1, 1,
                200.0)));
        }
        
        [Test]
        public void Operator_1x1_Multiply_1x2()
        {
            var A = Matrix.Create(1, 1,
                2.0);
            var B = Matrix.Create(1, 2,
                100.0, 200.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(1, 2,
                200.0, 400.0)));
        }
        
        [Test]
        public void Operator_1x1_Multiply_1x3()
        {
            var A = Matrix.Create(1, 1,
                2.0);
            var B = Matrix.Create(1, 3,
                100.0, 200.0, 300.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(1, 3,
                200.0, 400.0, 600.0)));
        }
        
        [Test]
        public void Operator_1x1_Multiply_1x4()
        {
            var A = Matrix.Create(1, 1,
                2.0);
            var B = Matrix.Create(1, 4,
                100.0, 200.0, 300.0, 400.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(1, 4,
                200.0, 400.0, 600.0, 800.0)));
        }
        
        #endregion A * B ===> (1x1 * 1xN)
        
        #region A * B ===> (1x2 * 2xN)
        // 1x2 * 2x1
        // 1x2 * 2x2
        // 1x2 * 2x3
        // 1x2 * 2x4
        
        [Test]
        public void Operator_1x2_Multiply_2x1()
        {
            var A = Matrix.Create(1, 2,
                2.0, 3.0);
            var B = Matrix.Create(2, 1,
                100.0,
                200.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(1, 1,
                800.0)));
        }
        
        [Test]
        public void Operator_1x2_Multiply_2x2()
        {
            var A = Matrix.Create(1, 2,
                2.0, 3.0);
            var B = Matrix.Create(2, 2,
                100.0, 200.0,
                300.0, 400.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(1, 2,
                1100.0, 1600.0)));
        }
        
        [Test]
        public void Operator_1x2_Multiply_2x3()
        {
            var A = Matrix.Create(1, 2,
                2.0, 3.0);
            var B = Matrix.Create(2, 3,
                100.0, 200.0, 300.0,
                400.0, 500.0, 600.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(1, 3,
                1400.0, 1900.0, 2400.0)));
        }
        
        [Test]
        public void Operator_1x2_Multiply_2x4()
        {
            var A = Matrix.Create(1, 2,
                2.0, 3.0);
            var B = Matrix.Create(2, 4,
                100.0, 200.0, 300.0, 400.0,
                500.0, 600.0, 700.0, 800.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(1, 4,
                1700.0, 2200.0, 2700.0, 3200.0)));
        }
        
        #endregion A * B ===> (1x2 * 2xN)
        
        #region A * B ===> (1x3 * 3xN)
        // 1x3 * 3x1
        // 1x3 * 3x2
        // 1x3 * 3x3
        // 1x3 * 3x4
        
        [Test]
        public void Operator_1x3_Multiply3x1()
        {
            var A = Matrix.Create(1, 3,
                2.0, 3.0, 4.0);
            var B = Matrix.Create(3, 1,
                100.0,
                200.0,
                300.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(1, 1,
                2000.0)));
        }
        
        [Test]
        public void Operator_1x3_Multiply_3x2()
        {
            var A = Matrix.Create(1, 3,
                2.0, 3.0, 4.0);
            var B = Matrix.Create(3, 2,
                100.0, 200.0, 300.0,
                400.0, 500.0, 600.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(1, 2,
                3100.0, 4000.0)));
        }
        
        [Test]
        public void Operator_1x3_Multiply_3x3()
        {
            var A = Matrix.Create(1, 3,
                2.0, 3.0, 4.0);
            var B = Matrix.Create(3, 3,
                100.0, 200.0, 300.0,
                400.0, 500.0, 600.0,
                700.0, 800.0, 900.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(1, 3,
                4200.0, 5100.0, 6000.0)));
        }
        
        [Test]
        public void Operator_1x3_Multiply_3x4()
        {
            var A = Matrix.Create(1, 3,
                2.0, 3.0, 4.0);
            var B = Matrix.Create(3, 4,
                100.0, 200.0, 300.0, 400.0,
                500.0, 600.0, 700.0, 800.0,
                900.0, 1000.0, 1100.0, 1200.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(1, 4,
                5300.0, 6200.0, 7100.0, 8000.0)));
        }
        
        #endregion A * B ===> (1x3 * 3xN)
          
        #region A * B ===> (1x4 * 4xN)
        // 1x4 * 4x1
        // 1x4 * 4x2
        // 1x4 * 4x3
        // 1x4 * 4x4
        
        [Test]
        public void Operator_1x4_Multiply_4x1()
        {
            var A = Matrix.Create(1, 4,
                2.0, 3.0, 4.0, 5.0);
            var B = Matrix.Create(4, 1,
                100.0,
                200.0,
                300.0,
                400.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(1, 1,
                4000.0)));
        }
        
        [Test]
        public void Operator_1x4_Multiply_4x2()
        {
            var A = Matrix.Create(1, 4,
                2.0, 3.0, 4.0, 5.0);
            var B = Matrix.Create(4, 2,
                100.0, 200.0,
                300.0, 400.0,
                500.0, 600.0,
                700.0, 800.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(1, 2,
                6600.0, 8000.0)));
        }
        
        [Test]
        public void Operator_1x4_Multiply_4x3()
        {
            var A = Matrix.Create(1, 4,
                2.0, 3.0, 4.0, 5.0);
            var B = Matrix.Create(4, 3,
                100.0, 200.0, 300.0,
                400.0, 500.0, 600.0,
                700.0, 800.0, 900.0,
                1000.0, 1100.0, 1200.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(1, 3,
                9200.0, 10600.0, 12000.0)));
        }
        
        [Test]
        public void Operator_1x4_Multiply_4x4()
        {
            var A = Matrix.Create(1, 4,
                2.0, 3.0, 4.0, 5.0);
            var B = Matrix.Create(4, 4,
                100.0, 200.0, 300.0, 400.0,
                500.0, 600.0, 700.0, 800.0,
                900.0, 1000.0, 1100.0, 1200.0,
                1300.0, 1400.0, 1500.0, 1600.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(1, 4,
                11800.0, 13200.0, 14600.0, 16000.0)));
        }
        
        #endregion A * B ===> (1x4 * 4xN)
        
        #region A * B ===> (2x1 * 1xN)
        // 2x1 * 1x1
        // 2x1 * 1x2
        // 2x1 * 1x3
        // 2x1 * 1x4
        
        [Test]
        public void Operator_2x1_Multiply_1x1()
        {
            var A = Matrix.Create(2, 1,
                2.0,
                3.0);
            var B = Matrix.Create(1, 1,
                100.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(2, 1,
                200.0,
                300.0)));
        }
        
        [Test]
        public void Operator_2x1_Multiply_1x2()
        {
            var A = Matrix.Create(2, 1,
                2.0,
                3.0);
            var B = Matrix.Create(1, 2,
                100.0, 200.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(2, 2,
                200.0, 400.0,
                300.0, 600.0)));
        }
        
        [Test]
        public void Operator_2x1_Multiply_1x3()
        {
            var A = Matrix.Create(2, 1,
                2.0,
                3.0);
            var B = Matrix.Create(1, 3,
                100.0, 200.0, 300.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(2, 3,
                200.0, 400.0, 600.0,
                300.0, 600.0, 900.0)));
        }
        
        [Test]
        public void Operator_2x1_Multiply_1x4()
        {
            var A = Matrix.Create(2, 1,
                2.0,
                3.0);
            var B = Matrix.Create(1, 4,
                100.0, 200.0, 300.0, 400.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(2, 4,
                200.0, 400.0, 600.0, 800.0,
                300.0, 600.0, 900.0, 1200.0)));
        }
        
        #endregion A * B ===> (2x1 * 1xN)
        
        #region A * B ===> (3x1 * 1xN)
        // 3x1 * 1x1
        // 3x1 * 1x2
        // 3x1 * 1x3
        // 3x1 * 1x4
        
        [Test]
        public void Operator_3x1_Multiply_1x1()
        {
            var A = Matrix.Create(3, 1,
                2.0,
                3.0,
                4.0);
            var B = Matrix.Create(1, 1,
                100.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(3, 1,
                200.0,
                300.0,
                400.0)));
        }
        
        [Test]
        public void Operator_3x1_Multiply_1x2()
        {
            var A = Matrix.Create(3, 1,
                2.0,
                3.0,
                4.0);
            var B = Matrix.Create(1, 2,
                100.0, 200.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(3, 2,
                200.0, 400.0,
                300.0, 600.0,
                400.0, 800.0)));
        }
        
        [Test]
        public void Operator_3x1_Multiply_1x3()
        {
            var A = Matrix.Create(3, 1,
                2.0,
                3.0,
                4.0);
            var B = Matrix.Create(1, 3,
                100.0, 200.0, 300.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(3, 3,
                200.0, 400.0, 600.0,
                300.0, 600.0, 900.0,
                400.0, 800.0, 1200.0)));
        }
        
        [Test]
        public void Operator_3x1_Multiply_1x4()
        {
            var A = Matrix.Create(3, 1,
                2.0,
                3.0,
                4.0);
            var B = Matrix.Create(1, 4,
                100.0, 200.0, 300.0, 400.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(3, 4,
                200.0, 400.0, 600.0, 800.0,
                300.0, 600.0, 900.0, 1200.0,
                400.0, 800.0, 1200.0, 1600.0)));
        }
        
        #endregion A * B ===> (3x1 * 1xN)
        
        #region A * B ===> (4x1 * 1xN)
        // 4x1 * 1x1
        // 4x1 * 1x2
        // 4x1 * 1x3
        // 4x1 * 1x4
        
        [Test]
        public void Operator_4x1_Multiply_1x1()
        {
            var A = Matrix.Create(4, 1,
                2.0,
                3.0,
                4.0,
                5.0);
            var B = Matrix.Create(1, 1,
                100.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(4, 1,
                200.0,
                300.0,
                400.0,
                500.0)));
        }
        
        [Test]
        public void Operator_4x1_Multiply_1x2()
        {
            var A = Matrix.Create(4, 1,
                2.0,
                3.0,
                4.0,
                5.0);
            var B = Matrix.Create(1, 2,
                100.0, 200.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(4, 2,
                200.0, 400.0,
                300.0, 600.0,
                400.0, 800.0,
                500.0, 1000.0)));
        }
        
        [Test]
        public void Operator_4x1_Multiply_1x3()
        {
            var A = Matrix.Create(4, 1,
                2.0,
                3.0,
                4.0,
                5.0);
            var B = Matrix.Create(1, 3,
                100.0, 200.0, 300.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(4, 3,
                200.0, 400.0, 600.0,
                300.0, 600.0, 900.0,
                400.0, 800.0, 1200.0,
                500.0, 1000.0, 1500.0)));
        }
        
        [Test]
        public void Operator_4x1_Multiply_1x4()
        {
            var A = Matrix.Create(4, 1,
                2.0,
                3.0,
                4.0,
                5.0);
            var B = Matrix.Create(1, 4,
                100.0, 200.0, 300.0, 400.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(4, 4,
                200.0, 400.0, 600.0, 800.0,
                300.0, 600.0, 900.0, 1200.0,
                400.0, 800.0, 1200.0, 1600.0,
                500.0, 1000.0, 1500.0, 2000.0)));
        }
        
        #endregion A * B ===> (4x1 * 1xN)
        
        #region A * B ===> (2x2 * 2xN)
        // 2x2 * 2x1
        // 2x2 * 2x2
        // 2x2 * 2x3
        // 2x2 * 2x4
        
        
        [Test]
        public void Operator_2x2_Multiply_2x1()
        {
            var A = Matrix.Create(2, 2,
                2.0, 3.0,
                4.0, 5.0);
            var B = Matrix.Create(2, 1,
                100.0,
                200.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(2, 1,
                800.0,
                1400.0)));
        }
        
        [Test]
        public void Operator_2x2_Multiply_2x2()
        {
            var A = Matrix.Create(2, 2,
                2.0, 3.0,
                4.0, 5.0);
            var B = Matrix.Create(2, 2,
                100.0, 200.0,
                300.0, 400.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(2, 2,
                1100.0, 1600.0,
                1900.0, 2800.0)));
        }
        
        [Test]
        public void Operator_2x2_Multiply_2x3()
        {
            var A = Matrix.Create(2, 2,
                2.0, 3.0,
                4.0, 5.0);
            var B = Matrix.Create(2, 3,
                100.0, 200.0, 300.0,
                400.0, 500.0, 600.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(2, 3,
                1400.0, 1900.0, 2400.0,
                2400.0, 3300.0, 4200.0)));
        }
        
        [Test]
        public void Operator_2x2_Multiply_2x4()
        {
            var A = Matrix.Create(2, 2,
                2.0, 3.0,
                4.0, 5.0);
            var B = Matrix.Create(2, 4,
                100.0, 200.0, 300.0, 400.0,
                500.0, 600.0, 700.0, 800.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(2, 4,
                1700.0, 2200.0, 2700.0, 3200.0,
                2900.0, 3800.0, 4700.0, 5600.0)));
        }
        
        #endregion A * B ===> (2x2 * 2xN)
        
        #region A * B ===> (3x2 * 2xN)
        // 3x2 * 2x1
        // 3x2 * 2x2
        // 3x2 * 2x3
        // 3x2 * 2x4
        
        [Test]
        public void Operator_3x2_Multiply_2x1()
        {
            var A = Matrix.Create(3, 2,
                2.0, 3.0,
                4.0, 5.0,
                6.0, 7.0);
            var B = Matrix.Create(2, 1,
                100.0,
                200.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(3, 1,
                800.0,
                1400.0,
                2000.0)));
        }
        
        [Test]
        public void Operator_3x2_Multiply_2x2()
        {
            var A = Matrix.Create(3, 2,
                2.0, 3.0,
                4.0, 5.0,
                6.0, 7.0);
            var B = Matrix.Create(2, 2,
                100.0, 200.0,
                300.0, 400.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(3, 2,
                1100.0, 1600.0,
                1900.0, 2800.0,
                2700.0, 4000.0)));
        }
        
        [Test]
        public void Operator_3x2_Multiply_2x3()
        {
            var A = Matrix.Create(3, 2,
                2.0, 3.0,
                4.0, 5.0,
                6.0, 7.0);
            var B = Matrix.Create(2, 3,
                100.0, 200.0, 300.0,
                400.0, 500.0, 600.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(3, 3,
                1400.0, 1900.0, 2400.0,
                2400.0, 3300.0, 4200.0,
                3400.0, 4700.0, 6000.0)));
        }
        
        [Test]
        public void Operator_3x2_Multiply_2x4()
        {
            var A = Matrix.Create(3, 2,
                2.0, 3.0,
                4.0, 5.0,
                6.0, 7.0);
            var B = Matrix.Create(2, 4,
                100.0, 200.0, 300.0, 400.0,
                500.0, 600.0, 700.0, 800.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(3, 4,
                1700.0, 2200.0, 2700.0, 3200.0,
                2900.0, 3800.0, 4700.0, 5600.0,
                4100.0, 5400.0, 6700.0, 8000.0)));
        }
        
        #endregion A * B ===> (3x2 * 2xN)
        
        #region A * B ===> (4x2 * 2xN)
        // 4x2 * 2x1
        // 4x2 * 2x2
        // 4x2 * 2x3
        // 4x2 * 2x4
        
        [Test]
        public void Operator_4x2_Multiply_2x1()
        {
            var A = Matrix.Create(4, 2,
                2.0, 3.0,
                4.0, 5.0,
                6.0, 7.0,
                8.0, 9.0);
            var B = Matrix.Create(2, 1,
                100.0,
                200.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(4, 1,
                800.0,
                1400.0,
                2000.0,
                2600.0)));
        }
        
        [Test]
        public void Operator_4x2_Multiply_2x2()
        {
            var A = Matrix.Create(4, 2,
                2.0, 3.0,
                4.0, 5.0,
                6.0, 7.0,
                8.0, 9.0);
            var B = Matrix.Create(2, 2,
                100.0, 200.0,
                300.0, 400.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(4, 2,
                1100.0, 1600.0,
                1900.0, 2800.0,
                2700.0, 4000.0,
                3500.0, 5200.0)));
        }
        
        [Test]
        public void Operator_4x2_Multiply_2x3()
        {
            var A = Matrix.Create(4, 2,
                2.0, 3.0,
                4.0, 5.0,
                6.0, 7.0,
                8.0, 9.0);
            var B = Matrix.Create(2, 3,
                100.0, 200.0, 300.0,
                400.0, 500.0, 600.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(4, 3,
                1400.0, 1900.0, 2400.0,
                2400.0, 3300.0, 4200.0,
                3400.0, 4700.0, 6000.0,
                4400.0, 6100.0, 7800.0)));
        }
        
        [Test]
        public void Operator_4x2_Multiply_2x4()
        {
            var A = Matrix.Create(4, 2,
                2.0, 3.0,
                4.0, 5.0,
                6.0, 7.0,
                8.0, 9.0);
            var B = Matrix.Create(2, 4,
                100.0, 200.0, 300.0, 400.0,
                500.0, 600.0, 700.0, 800.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(4, 4,
                1700.0, 2200.0, 2700.0, 3200.0,
                2900.0, 3800.0, 4700.0, 5600.0,
                4100.0, 5400.0, 6700.0, 8000.0,
                5300.0, 7000.0, 8700.0, 10400.0)));
        }
        
        #endregion A * B ===> (4x2 * 2xN)
            
        #region A * B ===> (2x3 * 3xN)
        // 2x3 * 3x1
        // 2x3 * 3x2
        // 2x3 * 3x3
        // 2x3 * 3x4
        
        [Test]
        public void Operator_2x3_Multiply_3x1()
        {
            var A = Matrix.Create(2, 3,
                2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0);
            var B = Matrix.Create(3, 1,
                100.0,
                200.0,
                300.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(2, 1,
                2000.0,
                3800.0)));
        }
        
        [Test]
        public void Operator_2x3_Multiply_3x2()
        {
            var A = Matrix.Create(2, 3,
                2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0);
            var B = Matrix.Create(3, 2,
                100.0, 200.0,
                300.0, 400.0,
                500.0, 600.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(2, 2,
                3100.0, 4000.0,
                5800.0, 7600.0)));
        }
        
        [Test]
        public void Operator_2x3_Multiply_3x3()
        {
            var A = Matrix.Create(2, 3,
                2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0);
            var B = Matrix.Create(3, 3,
                100.0, 200.0, 300.0,
                400.0, 500.0, 600.0,
                700.0, 800.0, 900.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(2, 3,
                4200.0, 5100.0, 6000.0,
                7800.0, 9600.0, 11400.0)));
        }
        
        [Test]
        public void Operator_2x3_Multiply_3x4()
        {
            var A = Matrix.Create(2, 3,
                2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0);
            var B = Matrix.Create(3, 4,
                100.0, 200.0, 300.0, 400.0,
                500.0, 600.0, 700.0, 800.0,
                900.0, 1000.0, 1100.0, 1200.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(2, 4,
                5300.0, 6200.0, 7100.0, 8000.0,
                9800.0, 11600.0, 13400.0, 15200.0)));
        }
        
        #endregion A * B ===> (2x3 * 3xN)
            
        #region A * B ===> (3x3 * 3xN)
        // 3x3 * 3x1
        // 3x3 * 3x2
        // 3x3 * 3x3
        // 3x3 * 3x4
        
        [Test]
        public void Operator_3x3_Multiply_3x1()
        {
            var A = Matrix.Create(3, 3,
                2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0,
                8.0, 9.0, 10.0);
            var B = Matrix.Create(3, 1,
                100.0,
                200.0,
                300.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(3, 1,
                2000.0,
                3800.0,
                5600.0)));
        }
        
        [Test]
        public void Operator_3x3_Multiply_3x2()
        {
            var A = Matrix.Create(3, 3,
                2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0,
                8.0, 9.0, 10.0);
            var B = Matrix.Create(3, 2,
                100.0, 200.0,
                300.0, 400.0,
                500.0, 600.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(3, 2,
                3100.0, 4000.0,
                5800.0, 7600.0,
                8500.0, 11200.0)));
        }
        
        [Test]
        public void Operator_3x3_Multiply_3x3()
        {
            var A = Matrix.Create(3, 3,
                2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0,
                8.0, 9.0, 10.0);
            var B = Matrix.Create(3, 3,
                100.0, 200.0, 300.0,
                400.0, 500.0, 600.0,
                700.0, 800.0, 900.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(3, 3,
                4200.0, 5100.0, 6000.0,
                7800.0, 9600.0, 11400.0,
                11400.0, 14100.0, 16800.0)));
        }
        
        [Test]
        public void Operator_3x3_Multiply_3x4()
        {
            var A = Matrix.Create(3, 3,
                2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0,
                8.0, 9.0, 10.0);
            var B = Matrix.Create(3, 4,
                100.0, 200.0, 300.0, 400.0,
                500.0, 600.0, 700.0, 800.0,
                900.0, 1000.0, 1100.0, 1200.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(3, 4,
                5300.0, 6200.0, 7100.0, 8000.0,
                9800.0, 11600.0, 13400.0, 15200.0,
                14300.0, 17000.0, 19700.0, 22400.0)));
        }
        
        #endregion A * B ===> (3x3 * 3xN)
            
        #region A * B ===> (4x3 * 3xN)
        // 4x3 * 3x1
        // 4x3 * 3x2
        // 4x3 * 3x3
        // 4x3 * 3x4
        
        [Test]
        public void Operator_4x3_Multiply_3x1()
        {
            var A = Matrix.Create(4, 3,
                2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0,
                8.0, 9.0, 10.0,
                11.0, 12.0, 13.0);
            var B = Matrix.Create(3, 1,
                100.0,
                200.0,
                300.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(4, 1,
                2000.0,
                3800.0,
                5600.0,
                7400.0)));
        }
        
        [Test]
        public void Operator_4x3_Multiply_3x2()
        {
            var A = Matrix.Create(4, 3,
                2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0,
                8.0, 9.0, 10.0,
                11.0, 12.0, 13.0);
            var B = Matrix.Create(3, 2,
                100.0, 200.0,
                300.0, 400.0,
                500.0, 600.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(4, 2,
                3100.0, 4000.0,
                5800.0, 7600.0,
                8500.0, 11200.0,
                11200.0, 14800.0)));
        }
        
        [Test]
        public void Operator_4x3_Multiply_3x3()
        {
            var A = Matrix.Create(4, 3,
                2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0,
                8.0, 9.0, 10.0,
                11.0, 12.0, 13.0);
            var B = Matrix.Create(3, 3,
                100.0, 200.0, 300.0,
                400.0, 500.0, 600.0,
                700.0, 800.0, 900.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(4, 3,
                4200.0, 5100.0, 6000.0,
                7800.0, 9600.0, 11400.0,
                11400.0, 14100.0, 16800.0,
                15000.0, 18600.0, 22200.0)));
        }
        
        [Test]
        public void Operator_4x3_Multiply_3x4()
        {
            var A = Matrix.Create(4, 3,
                2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0,
                8.0, 9.0, 10.0,
                11.0, 12.0, 13.0);
            var B = Matrix.Create(3, 4,
                100.0, 200.0, 300.0, 400.0,
                500.0, 600.0, 700.0, 800.0,
                900.0, 1000.0, 1100.0, 1200.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(4, 4,
                5300.0, 6200.0, 7100.0, 8000.0,
                9800.0, 11600.0, 13400.0, 15200.0,
                14300.0, 17000.0, 19700.0, 22400.0,
                18800.0, 22400.0, 26000.0, 29600.0)));
        }
        
        #endregion A * B ===> (4x3 * 3xN)
            
        #region A * B ===> (2x4 * 4xN) 
        // 2x4 * 4x1
        // 2x4 * 4x2
        // 2x4 * 4x3
        // 2x4 * 4x4
        
        [Test]
        public void Operator_2x4_Multiply_4x1()
        {
            var A = Matrix.Create(2, 4,
                2.0, 3.0, 4.0, 5.0, 
                6.0, 7.0, 8.0, 9.0);
            var B = Matrix.Create(4, 1,
                100.0,
                200.0,
                300.0,
                400.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(2, 1,
                4000.0,
                8000.0)));
        }
        
        [Test]
        public void Operator_2x4_Multiply_4x2()
        {
            var A = Matrix.Create(2, 4,
                2.0, 3.0, 4.0, 5.0, 
                6.0, 7.0, 8.0, 9.0);
            var B = Matrix.Create(4, 2,
                100.0, 200.0,
                300.0, 400.0,
                500.0, 600.0,
                700.0, 800.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(2, 2,
                6600.0, 8000.0,
                13000.0, 16000.0)));
        }
        
        [Test]
        public void Operator_2x4_Multiply_4x3()
        {
            var A = Matrix.Create(2, 4,
                2.0, 3.0, 4.0, 5.0, 
                6.0, 7.0, 8.0, 9.0);
            var B = Matrix.Create(4, 3,
                100.0, 200.0, 300.0,
                400.0, 500.0, 600.0,
                700.0, 800.0, 900.0,
                1000.0, 1100.0, 1200.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(2, 3,
                9200.0, 10600.0, 12000.0,
                18000.0, 21000.0, 24000.0)));
        }
        
        [Test]
        public void Operator_2x4_Multiply_4x4()
        {
            var A = Matrix.Create(2, 4,
                2.0, 3.0, 4.0, 5.0, 
                6.0, 7.0, 8.0, 9.0);
            var B = Matrix.Create(4, 4,
                100.0, 200.0, 300.0, 400.0,
                500.0, 600.0, 700.0, 800.0,
                900.0, 1000.0, 1100.0, 1200.0,
                1300.0, 1400.0, 1500.0, 1600.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(2, 4,
                11800.0, 13200.0, 14600.0, 16000.0,
                23000.0, 26000.0, 29000.0, 32000.0)));
        }
        
        #endregion A * B ===> (2x4 * 4xN) 
            
        #region A * B ===> (3x4 * 4xN) 
        // 3x4 * 4x1
        // 3x4 * 4x2
        // 3x4 * 4x3
        // 3x4 * 4x4
        
        [Test]
        public void Operator_3x4_Multiply_4x1()
        {
            var A = Matrix.Create(3, 4,
                2.0, 3.0, 4.0, 5.0, 
                6.0, 7.0, 8.0, 9.0,
                10.0, 11.0, 12.0, 13.0);
            var B = Matrix.Create(4, 1,
                100.0,
                200.0,
                300.0,
                400.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(3, 1,
                4000.0,
                8000.0,
                12000.0)));
        }
        
        [Test]
        public void Operator_3x4_Multiply_4x2()
        {
            var A = Matrix.Create(3, 4,
                2.0, 3.0, 4.0, 5.0, 
                6.0, 7.0, 8.0, 9.0,
                10.0, 11.0, 12.0, 13.0);
            var B = Matrix.Create(4, 2,
                100.0, 200.0,
                300.0, 400.0,
                500.0, 600.0,
                700.0, 800.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(3, 2,
                6600.0, 8000.0,
                13000.0, 16000.0,
                19400.0, 24000.0)));
        }
        
        [Test]
        public void Operator_3x4_Multiply_4x3()
        {
            var A = Matrix.Create(3, 4,
                2.0, 3.0, 4.0, 5.0, 
                6.0, 7.0, 8.0, 9.0,
                10.0, 11.0, 12.0, 13.0);
            var B = Matrix.Create(4, 3,
                100.0, 200.0, 300.0,
                400.0, 500.0, 600.0,
                700.0, 800.0, 900.0,
                1000.0, 1100.0, 1200.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(3, 3,
                9200.0, 10600.0, 12000.0,
                18000.0, 21000.0, 24000.0,
                26800.0, 31400.0, 36000.0)));
        }
        
        [Test]
        public void Operator_3x4_Multiply_4x4()
        {
            var A = Matrix.Create(3, 4,
                2.0, 3.0, 4.0, 5.0, 
                6.0, 7.0, 8.0, 9.0,
                10.0, 11.0, 12.0, 13.0);
            var B = Matrix.Create(4, 4,
                100.0, 200.0, 300.0, 400.0,
                500.0, 600.0, 700.0, 800.0,
                900.0, 1000.0, 1100.0, 1200.0,
                1300.0, 1400.0, 1500.0, 1600.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(3, 4,
                11800.0, 13200.0, 14600.0, 16000.0,
                23000.0, 26000.0, 29000.0, 32000.0,
                34200.0, 38800.0, 43400.0, 48000.0)));
        }
        
        #endregion A * B ===> (3x4 * 4xN) 
            
        #region A * B ===> (4x4 * 4xN) 
        // 4x4 * 4x1
        // 4x4 * 4x2
        // 4x4 * 4x3
        // 4x4 * 4x4
        
        [Test]
        public void Operator_4x4_Multiply_4x1()
        {
            var A = Matrix.Create(4, 4,
                2.0, 3.0, 4.0, 5.0, 
                6.0, 7.0, 8.0, 9.0,
                10.0, 11.0, 12.0, 13.0,
                14.0, 15.0, 16.0, 17.0);
            var B = Matrix.Create(4, 1,
                100.0,
                200.0,
                300.0,
                400.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(4, 1,
                4000.0,
                8000.0,
                12000.0,
                16000.0)));
        }
        
        [Test]
        public void Operator_4x4_Multiply_4x2()
        {
            var A = Matrix.Create(4, 4,
                2.0, 3.0, 4.0, 5.0, 
                6.0, 7.0, 8.0, 9.0,
                10.0, 11.0, 12.0, 13.0,
                14.0, 15.0, 16.0, 17.0);
            var B = Matrix.Create(4, 2,
                100.0, 200.0,
                300.0, 400.0,
                500.0, 600.0,
                700.0, 800.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(4, 2,
                6600.0, 8000.0,
                13000.0, 16000.0,
                19400.0, 24000.0,
                25800.0, 32000.0)));
        }
        
        [Test]
        public void Operator_4x4_Multiply_4x3()
        {
            var A = Matrix.Create(4, 4,
                2.0, 3.0, 4.0, 5.0, 
                6.0, 7.0, 8.0, 9.0,
                10.0, 11.0, 12.0, 13.0,
                14.0, 15.0, 16.0, 17.0);
            var B = Matrix.Create(4, 3,
                100.0, 200.0, 300.0,
                400.0, 500.0, 600.0,
                700.0, 800.0, 900.0,
                1000.0, 1100.0, 1200.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(4, 3,
                9200.0, 10600.0, 12000.0,
                18000.0, 21000.0, 24000.0,
                26800.0, 31400.0, 36000.0,
                35600.0, 41800.0, 48000.0)));
        }
        
        [Test]
        public void Operator_4x4_Multiply_4x4()
        {
            var A = Matrix.Create(4, 4,
                2.0, 3.0, 4.0, 5.0, 
                6.0, 7.0, 8.0, 9.0,
                10.0, 11.0, 12.0, 13.0,
                14.0, 15.0, 16.0, 17.0);
            var B = Matrix.Create(4, 4,
                100.0, 200.0, 300.0, 400.0,
                500.0, 600.0, 700.0, 800.0,
                900.0, 1000.0, 1100.0, 1200.0,
                1300.0, 1400.0, 1500.0, 1600.0);

            var res = A * B;
            
            Assert.That(res, Is.EqualTo(Matrix.Create(4, 4,
                11800.0, 13200.0, 14600.0, 16000.0,
                23000.0, 26000.0, 29000.0, 32000.0,
                34200.0, 38800.0, 43400.0, 48000.0,
                45400.0, 51600.0, 57800.0, 64000.0)));
        }
        
        #endregion A * B ===> (4x4 * 4xN) 
        
        #endregion A * B
        
        #region A ^ x (Power - Multiply matrix x times)
        
        [Test]
        public void Power_With_0_Returns_IdentityMatrix(
            [Range(0, 4)] int cols,
            [Range(0, 4)] int rows)
        {
            var m = Matrix.Create(rows, cols);
            
            var p = Matrix.Power(m, 0);
                
            Assert.That(p, Is.EqualTo(Matrix.IdentityMatrix(rows, cols)));
        }
        
        [Test]
        public void Power_With_1_Returns_SameMatrix(
            [Range(0, 4)] int cols,
            [Range(0, 4)] int rows)
        {
            var values = Enumerable.Range(0, rows * cols).Select(x => (double)x).ToArray();
            var m = Matrix.Create(rows, cols, values);
            
            var p = Matrix.Power(m, 1);
                
            Assert.That(p, Is.EqualTo(m));
        }
        
        [Test]
        public void Power_Throws_With_NonSquareMatrix(
            [Range(2, 5)] int pow)
        {
            var newM = (int rows, int cols) => Matrix.Create(rows, cols);

            Assert.Throws<MException>(() => Matrix.Power(newM(0, 1), pow));
            Assert.Throws<MException>(() => Matrix.Power(newM(0, 2), pow));
            Assert.Throws<MException>(() => Matrix.Power(newM(0, 3), pow));
            Assert.Throws<MException>(() => Matrix.Power(newM(0, 4), pow));
            
            Assert.Throws<MException>(() => Matrix.Power(newM(1, 0), pow));
            Assert.Throws<MException>(() => Matrix.Power(newM(1, 2), pow));
            Assert.Throws<MException>(() => Matrix.Power(newM(1, 3), pow));
            Assert.Throws<MException>(() => Matrix.Power(newM(1, 4), pow));
            
            Assert.Throws<MException>(() => Matrix.Power(newM(2, 0), pow));
            Assert.Throws<MException>(() => Matrix.Power(newM(2, 1), pow));
            Assert.Throws<MException>(() => Matrix.Power(newM(2, 3), pow));
            Assert.Throws<MException>(() => Matrix.Power(newM(2, 4), pow));
            
            Assert.Throws<MException>(() => Matrix.Power(newM(3, 0), pow));
            Assert.Throws<MException>(() => Matrix.Power(newM(3, 1), pow));
            Assert.Throws<MException>(() => Matrix.Power(newM(3, 2), pow));
            Assert.Throws<MException>(() => Matrix.Power(newM(3, 4), pow));
            
            Assert.Throws<MException>(() => Matrix.Power(newM(4, 0), pow));
            Assert.Throws<MException>(() => Matrix.Power(newM(4, 1), pow));
            Assert.Throws<MException>(() => Matrix.Power(newM(4, 2), pow));
            Assert.Throws<MException>(() => Matrix.Power(newM(4, 3), pow));
        }
        
        [Test]
        public void Power_DoesNotThrows_With_SquareMatrix(
            [Range(0, 4)] int size,
            [Range(2, 5)] int pow)
        {
            var m = Matrix.Create(size, size);

            var p = Matrix.Power(m, pow);
            
            Assert.That(p.cols, Is.EqualTo(size));
            Assert.That(p.rows, Is.EqualTo(size));
        }
        
        [Test]
        public void Power_ZeroMatrix_DoesNotChange(
            [Range(0, 5)] int size,
            [Range(2, 5)] int pow)
        {
            var m = Matrix.ZeroMatrix(size, size);

            Assert.That(Matrix.Power(m, pow), Is.EqualTo(Matrix.ZeroMatrix(size, size)));
        }
        
        [Test]
        public void Power_IdentityMatrix_DoesNotChange(
            [Range(0, 5)] int size,
            [Range(2, 5)] int pow)
        {
            var m = Matrix.IdentityMatrix(size, size);

            Assert.That(Matrix.Power(m, pow), Is.EqualTo(Matrix.IdentityMatrix(size, size)));
        }
        
        [Test]
        public void Power_0x0(
            [Range(0, 5)] int pow)
        {
            var m = Matrix.Create(0, 0);

            Assert.That(Matrix.Power(m, pow), Is.EqualTo(Matrix.ZeroMatrix(0, 0)));
        }
        
        [Test]
        public void Power_1x1()
        {
            var m = Matrix.Create(1, 1,
                5.0);

            Assert.That(Matrix.Power(m, 0), Is.EqualTo(Matrix.Create(1, 1,
                1.0)));
            Assert.That(Matrix.Power(m, 1), Is.EqualTo(Matrix.Create(1, 1,
                5.0)));
            Assert.That(Matrix.Power(m, 2), Is.EqualTo(Matrix.Create(1, 1,
                25.0)));
            Assert.That(Matrix.Power(m, 3), Is.EqualTo(Matrix.Create(1, 1,
                125.0)));
            Assert.That(Matrix.Power(m, 4), Is.EqualTo(Matrix.Create(1, 1,
                625.0)));
            Assert.That(Matrix.Power(m, 5), Is.EqualTo(Matrix.Create(1, 1,
                3125.0)));
        }
        
        [Test]
        public void Power_2x2()
        {
            var m = Matrix.Create(2, 2,
                2.0, 3.0,
                4.0, 5.0);
            
            Assert.That(Matrix.Power(m, 0), Is.EqualTo(Matrix.IdentityMatrix(2, 2)));
            Assert.That(Matrix.Power(m, 1), Is.EqualTo(Matrix.Create(2, 2,
                2.0, 3.0,
                4.0, 5.0)));
            Assert.That(Matrix.Power(m, 2), Is.EqualTo(Matrix.Create(2, 2,
                16.0, 21.0,
                28.0, 37.0)));
            Assert.That(Matrix.Power(m, 3), Is.EqualTo(Matrix.Create(2, 2,
                116.0, 153.0,
                204.0, 269.0)));
            Assert.That(Matrix.Power(m, 4), Is.EqualTo(Matrix.Create(2, 2,
                844.0, 1113.0,
                1484.0, 1957.0)));
            Assert.That(Matrix.Power(m, 5), Is.EqualTo(Matrix.Create(2, 2,
                6140.0, 8097.0,
                10796.0, 14237.0)));
        }
        
        [Test]
        public void Power_3x3()
        {
            var m = Matrix.Create(3, 3,
                2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0,
                8.0, 9.0, 10.0);

            Assert.That(Matrix.Power(m, 0), Is.EqualTo(Matrix.IdentityMatrix(3, 3)));
            Assert.That(Matrix.Power(m, 1), Is.EqualTo(Matrix.Create(3, 3,
                2.0, 3.0, 4.0,
                5.0, 6.0, 7.0,
                8.0, 9.0, 10.0)));
            Assert.That(Matrix.Power(m, 2), Is.EqualTo(Matrix.Create(3, 3,
                51.0, 60.0, 69.0,
                96.0, 114.0, 132.0,
                141.0, 168.0, 195.0)));
            Assert.That(Matrix.Power(m, 3), Is.EqualTo(Matrix.Create(3, 3,
                954.0, 1134.0, 1314.0,
                1818.0, 2160.0, 2502.0,
                2682.0, 3186.0, 3690.0)));
            Assert.That(Matrix.Power(m, 4), Is.EqualTo(Matrix.Create(3, 3,
                18090.0, 21492.0, 24894.0,
                34452.0, 40932.0, 47412.0,
                50814.0, 60372.0, 69930.0)));
            Assert.That(Matrix.Power(m, 5), Is.EqualTo(Matrix.Create(3, 3,
                342792.0, 407268.0, 471744.0,
                652860.0, 775656.0, 898452.0,
                962928.0, 1144044.0, 1325160.0)));
        }
        
        [Test]
        public void Power_4x4()
        {
            var m = Matrix.Create(4, 4,
                2.0, 3.0, 4.0, 5.0, 
                6.0, 7.0, 8.0, 9.0, 
                10.0, 11.0, 12.0, 13.0,
                14.0, 15.0, 16.0, 17.0);

            Assert.That(Matrix.Power(m, 0), Is.EqualTo(Matrix.IdentityMatrix(4, 4)));
            Assert.That(Matrix.Power(m, 1), Is.EqualTo(Matrix.Create(4, 4,
                2.0, 3.0, 4.0, 5.0, 
                6.0, 7.0, 8.0, 9.0, 
                10.0, 11.0, 12.0, 13.0,
                14.0, 15.0, 16.0, 17.0)));
            Assert.That(Matrix.Power(m, 2), Is.EqualTo(Matrix.Create(4, 4,
                132.0, 146.0, 160.0, 174.0, 
                260.0, 290.0, 320.0, 350.0, 
                388.0, 434.0, 480.0, 526.0,
                516.0, 578.0, 640.0, 702.0)));
            Assert.That(Matrix.Power(m, 3), Is.EqualTo(Matrix.Create(4, 4,
                5176.0, 5788.0, 6400.0, 7012.0, 
                10360.0, 11580.0, 12800.0, 14020.0, 
                15544.0, 17372.0, 19200.0, 21028.0,
                20728.0, 23164.0, 25600.0, 28036.0)));
            Assert.That(Matrix.Power(m, 4), Is.EqualTo(Matrix.Create(4, 4,
                207248.0, 231624.0, 256000.0, 280376.0, 
                414480.0, 463240.0, 512000.0, 560760.0, 
                621712.0, 694856.0, 768000.0, 841144.0,
                828944.0, 926472.0, 1024000.0, 1121528.0)));
            Assert.That(Matrix.Power(m, 5), Is.EqualTo(Matrix.Create(4, 4,
                8289504.0, 9264752.0, 10240000.0, 11215248.0, 
                16579040.0, 18529520.0, 20480000.0, 22430480.0, 
                24868576.0, 27794288.0, 30720000.0, 33645712.0,
                33158112.0, 37059056.0, 40960000.0, 44860944.0)));
        }
        
        [Test]
        public void Power_1x1_NegativePow()
        {
            var m = Matrix.Create(1, 1,
                5.0);

            Assert.That(Matrix.Power(m, -1), Is.EqualTo(Matrix.Create(1, 1,
                0.2)));
            Assert.That(Matrix.Power(m, -2), Is.EqualTo(Matrix.Create(1, 1,
                0.04)));
            Assert.That(Matrix.Power(m, -3), Is.EqualTo(Matrix.Create(1, 1,
                0.008)));
            Assert.That(Matrix.Power(m, -4), Is.EqualTo(Matrix.Create(1, 1,
                0.0016)));
            Assert.That(Matrix.Power(m, -5), Is.EqualTo(Matrix.Create(1, 1,
                0.00032)));
        }
        
        [Test]
        public void Power_2x2_NegativePow()
        {
            var m = Matrix.Create(2, 2,
                2.0, 3.0,
                4.0, 5.0);
            
            Assert.That(Matrix.Power(m, -1), Is.EqualTo(Matrix.Create(2, 2,
                -2.5, 1.5,
                2.0, -1.0)));
            Assert.That(Matrix.Power(m, -2), Is.EqualTo(Matrix.Create(2, 2,
                9.25, -5.25,
                -7.0, 4.0)));
            Assert.That(Matrix.Power(m, -3), Is.EqualTo(Matrix.Create(2, 2,
                -33.625, 19.125,
                25.5, -14.5)));
            Assert.That(Matrix.Power(m, -4), Is.EqualTo(Matrix.Create(2, 2,
                122.3125, -69.5625,
                -92.75, 52.75)));
            Assert.That(Matrix.Power(m, -5), Is.EqualTo(Matrix.Create(2, 2,
                -444.90625, 253.03125,
                337.375, -191.875)));
        }
        
        [Test]
        public void Power_3x3_NegativePow()
        {
            var m = Matrix.Create(3, 3,
                1.0, 2.0, 3.0, 
                0.0, 1.0, 4.0,
                5.0, 6.0, 0.0);
            
            Assert.That(Matrix.Power(m, -1), Is.EqualTo(Matrix.Create(3, 3,
                -24.0, 18.0, 5.0,
                20.0, -15.0, -4.0,
                -5.0, 4.0, 1.0)));
            Assert.That(Matrix.Power(m, -2), Is.EqualTo(Matrix.Create(3, 3,
                911.0, -682.0, -187.0,
                -760.0, 569.0, 156.0,
                195.0, -146.0, -40.0)));
            Assert.That(Matrix.Power(m, -3), Is.EqualTo(Matrix.Create(3, 3,
                -34569.0, 25880.0, 7096.0,
                28840.0, -21591.0, -5920.0,
                -7400.0, 5540.0, 1519.0)));
            Assert.That(Matrix.Power(m, -4), Is.EqualTo(Matrix.Create(3, 3,
                1311776.0, -982058.0, -269269.0,
                -1094380.0, 819305.0, 224644.0,
                280805.0, -210224.0, -57641.0)));
            Assert.That(Matrix.Power(m, -5), Is.EqualTo(Matrix.Create(3, 3,
                -49777439.0, 37265762.0, 10217843.0,
                41528000.0, -31089839.0, -8524476.0,
                -10655595.0, 7977286.0, 2187280.0)));
        }
        
        [Test]
        public void Power_4x4_NegativePow()
        {
            var m = Matrix.Create(4, 4,
                1.0, 1.0, 1.0, -1.0, 
                1.0, 1.0, -1.0, 1.0, 
                1.0, -1.0, 1.0, 1.0,
                -1.0, 1.0, 1.0, 1.0);

            Assert.That(Matrix.Power(m, -1), Is.EqualTo(Matrix.Create(4, 4,
                0.25, 0.25, 0.25, -0.25,
                0.25, 0.25, -0.25, 0.25,
                0.25, -0.25, 0.25, 0.25,
                -0.25, 0.25, 0.25, 0.25)));
            Assert.That(Matrix.Power(m, -2), Is.EqualTo(Matrix.Create(4, 4,
                0.25, 0.0, 0.0, 0.0,
                0.0, 0.25, 0.0, 0.0,
                0.0, 0.0, 0.25, 0.0,
                0.0, 0.0, 0.0, 0.25)));
            Assert.That(Matrix.Power(m, -3), Is.EqualTo(Matrix.Create(4, 4,
                0.0625, 0.0625, 0.0625, -0.0625,
                0.0625, 0.0625, -0.0625, 0.0625,
                0.0625, -0.0625, 0.0625, 0.0625,
                -0.0625, 0.0625, 0.0625, 0.0625)));
            Assert.That(Matrix.Power(m, -4), Is.EqualTo(Matrix.Create(4, 4,
                0.0625, 0.0, 0.0, 0.0,
                0.0, 0.0625, 0.0, 0.0,
                0.0, 0.0, 0.0625, 0.0,
                0.0, 0.0, 0.0, 0.0625)));
            Assert.That(Matrix.Power(m, -5), Is.EqualTo(Matrix.Create(4, 4,
                0.015625, 0.015625, 0.015625, -0.015625,
                0.015625, 0.015625, -0.015625, 0.015625,
                0.015625, -0.015625, 0.015625, 0.015625,
                -0.015625, 0.015625, 0.015625, 0.015625)));
        }
        
        #endregion A ^ x (Power)
        
        #region ToString
        
        [Test]
        public void ToString_0x0()
        {
            var m = Matrix.Create(0, 0);

            Assert.That(m.ToString(), Is.EqualTo(string.Empty));
        }
        
        [Test]
        public void ToString_1x1()
        {
            var m = Matrix.Create(1, 1,
                1.0);

            var expected = " 1.00 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_1x1_RoundedTo2DigitsAfterDecimalPoint()
        {
            var m = Matrix.Create(1, 1,
                1.5678);

            var expected = " 1.57 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_1x1_MoreDigits()
        {
            var m = Matrix.Create(1, 1,
                1234.5678);

            var expected = "1234.57 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_1x2()
        {
            var m = Matrix.Create(1, 2,
                1.0, 2.0);

            var expected = " 1.00  2.00 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_1x2_RoundedTo2DigitsAfterDecimalPoint()
        {
            var m = Matrix.Create(1, 2,
                1.123, 2.567);

            var expected = " 1.12  2.57 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_1x2_MoreDigits()
        {
            var m = Matrix.Create(1, 2,
                1234.123, 23456.567);

            var expected = "1234.12 23456.57 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_1x3()
        {
            var m = Matrix.Create(1, 3,
                1.0, 2.0, 3.0);

            var expected = " 1.00  2.00  3.00 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_1x3_RoundedTo2DigitsAfterDecimalPoint()
        {
            var m = Matrix.Create(1, 3,
                1.123, 2.567, 3.987);

            var expected = " 1.12  2.57  3.99 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_1x3_MoreDigits()
        {
            var m = Matrix.Create(1, 3,
                123.123, 2345.567, 34567.987);

            var expected = "123.12 2345.57 34567.99 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_1x4()
        {
            var m = Matrix.Create(1, 4,
                1.0, 2.0, 3.0, 4.0);

            var expected = " 1.00  2.00  3.00  4.00 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_1x4_RoundedTo2DigitsAfterDecimalPoint()
        {
            var m = Matrix.Create(1, 4,
                1.123, 2.567, 3.987, 4.543);

            var expected = " 1.12  2.57  3.99  4.54 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_1x4_MoreDigits()
        {
            var m = Matrix.Create(1, 4,
                123.123, 2345.567, 34567.987, 456789.543);

            var expected = "123.12 2345.57 34567.99 456789.54 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_2x1()
        {
            var m = Matrix.Create(2, 1,
                1.0, 
                2.0);

            var expected = " 1.00 " + Environment.NewLine +
                           " 2.00 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_2x1_RoundedTo2DigitsAfterDecimalPoint()
        {
            var m = Matrix.Create(2, 1,
                1.123, 
                2.567);

            var expected = " 1.12 " + Environment.NewLine +
                           " 2.57 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_2x1_MoreDigits()
        {
            var m = Matrix.Create(2, 1,
                123.123, 
                2345.567);

            var expected = "123.12 " + Environment.NewLine +
                           "2345.57 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_2x2()
        {
            var m = Matrix.Create(2, 2,
                1.0, 2.0,
                3.0, 4.0);

            var expected = " 1.00  2.00 " + Environment.NewLine +
                           " 3.00  4.00 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_2x2_RoundedTo2DigitsAfterDecimalPoint()
        {
            var m = Matrix.Create(2, 2,
                1.12345, 2.98765,
                3.55555, 4.44444);

            var expected = " 1.12  2.99 " + Environment.NewLine + 
                           " 3.56  4.44 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_2x2_MoreDigits()
        {
            var m = Matrix.Create(2, 2,
                123.12345, 2345.98765,
                34567.55555, 456789.44444);

            var expected = "123.12 2345.99 " + Environment.NewLine + 
                           "34567.56 456789.44 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_2x3()
        {
            var m = Matrix.Create(2, 3,
                1.0, 2.0, 3.0, 
                4.0, 5.0, 6.0);

            var expected = " 1.00  2.00  3.00 " + Environment.NewLine +
                           " 4.00  5.00  6.00 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_2x3_RoundedTo2DigitsAfterDecimalPoint()
        {
            var m = Matrix.Create(2, 3,
                1.12345, 2.98765, 3.55555, 
                4.44444, 5.32490, 6.009);

            var expected = " 1.12  2.99  3.56 " + Environment.NewLine + 
                           " 4.44  5.32  6.01 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_2x3_MoreDigits()
        {
            var m = Matrix.Create(2, 3,
                111.12345, 2222.98765, 33333.55555, 
                444444.44444, 5555555.32490, 66666666.009);

            var expected = "111.12 2222.99 33333.56 " + Environment.NewLine + 
                           "444444.44 5555555.32 66666666.01 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_2x4()
        {
            var m = Matrix.Create(2, 4,
                1.0, 2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0, 8.0);

            var expected = " 1.00  2.00  3.00  4.00 " + Environment.NewLine +
                           " 5.00  6.00  7.00  8.00 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_2x4_RoundedTo2DigitsAfterDecimalPoint()
        {
            var m = Matrix.Create(2, 4,
                1.12345, 2.98765, 3.55555, 4.44444, 
                5.32490, 6.009, 7.099, 8.34568);

            var expected = " 1.12  2.99  3.56  4.44 " + Environment.NewLine + 
                           " 5.32  6.01  7.10  8.35 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_2x4_MoreDigits()
        {
            var m = Matrix.Create(2, 4,
                111.12345, 2222.98765, 33333.55555, 444444.44444, 
                5555555.32490, 66666666.009, 777777777.099, 888888888.34568);

            var expected = "111.12 2222.99 33333.56 444444.44 " + Environment.NewLine + 
                           "5555555.32 66666666.01 777777777.10 888888888.35 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_3x1()
        {
            var m = Matrix.Create(3, 1,
                1.0, 
                2.0, 
                3.0);

            var expected = " 1.00 " + Environment.NewLine +
                           " 2.00 " + Environment.NewLine +
                           " 3.00 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_3x1_RoundedTo2DigitsAfterDecimalPoint()
        {
            var m = Matrix.Create(3, 1,
                1.123, 
                2.567, 
                3.987);

            var expected = " 1.12 " + Environment.NewLine +
                           " 2.57 " + Environment.NewLine +
                           " 3.99 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_3x1_MoreDigits()
        {
            var m = Matrix.Create(3, 1,
                111.123, 
                2222.567, 
                33333.987);

            var expected = "111.12 " + Environment.NewLine +
                           "2222.57 " + Environment.NewLine +
                           "33333.99 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_3x2()
        {
            var m = Matrix.Create(3, 2,
                1.0, 2.0, 
                3.0, 4.0, 
                5.0, 6.0);

            var expected = " 1.00  2.00 " + Environment.NewLine +
                           " 3.00  4.00 " + Environment.NewLine +
                           " 5.00  6.00 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_3x2_RoundedTo2DigitsAfterDecimalPoint()
        {
            var m = Matrix.Create(3, 2,
                1.12345, 2.98765, 
                3.55555, 4.44444, 
                5.32490, 6.009);

            var expected = " 1.12  2.99 " + Environment.NewLine + 
                           " 3.56  4.44 " + Environment.NewLine + 
                           " 5.32  6.01 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_3x2_MoreDigits()
        {
            var m = Matrix.Create(3, 2,
                111.12345, 2222.98765, 
                33333.55555, 444444.44444, 
                5555555.32490, 66666666.009);

            var expected = "111.12 2222.99 " + Environment.NewLine + 
                           "33333.56 444444.44 " + Environment.NewLine + 
                           "5555555.32 66666666.01 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_3x3()
        {
            var m = Matrix.Create(3, 3,
                1.0, 2.0, 3.0, 
                4.0, 5.0, 6.0, 
                7.0, 8.0, 9.0);

            var expected = " 1.00  2.00  3.00 " + Environment.NewLine +
                           " 4.00  5.00  6.00 " + Environment.NewLine +
                           " 7.00  8.00  9.00 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_3x3_RoundedTo2DigitsAfterDecimalPoint()
        {
            var m = Matrix.Create(3, 3,
                1.12345, 2.98765, 3.55555, 
                4.44444, 5.32490, 6.009, 
                7.099, 8.34568, 9.9999);

            var expected = " 1.12  2.99  3.56 " + Environment.NewLine + 
                           " 4.44  5.32  6.01 " + Environment.NewLine + 
                           " 7.10  8.35 10.00 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_3x3_MoreDigits()
        {
            var m = Matrix.Create(3, 3,
                111.12345, 2222.98765, 33333.55555, 
                444444.44444, 5555555.32490, 66666666.009, 
                777777777.099, 88888888888.34568, 9999999999.9999);

            var expected = "111.12 2222.99 33333.56 " + Environment.NewLine + 
                           "444444.44 5555555.32 66666666.01 " + Environment.NewLine + 
                           "777777777.10 88888888888.35 10000000000.00 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_3x4()
        {
            var m = Matrix.Create(3, 4,
                1.0, 2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0, 8.0, 
                9.0, 10.0, 11.0, 12.0);

            var expected = " 1.00  2.00  3.00  4.00 " + Environment.NewLine +
                           " 5.00  6.00  7.00  8.00 " + Environment.NewLine +
                           " 9.00 10.00 11.00 12.00 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_3x4_RoundedTo2DigitsAfterDecimalPoint()
        {
           
            var m = Matrix.Create(3, 4,
                1.12345, 2.98765, 3.55555, 4.44444, 
                5.32490, 6.009, 7.099, 8.34568, 
                9.9999, 10.10101, 11.11111, 12.121212);

            var expected = " 1.12  2.99  3.56  4.44 " + Environment.NewLine + 
                           " 5.32  6.01  7.10  8.35 " + Environment.NewLine + 
                           "10.00 10.10 11.11 12.12 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_3x4_MoreDigits()
        {
           
            var m = Matrix.Create(3, 4,
                111.12345, 2222.98765, 33333.55555, 444444.44444, 
                5555555.32490, 66666666.009, 777777777.099, 8888888888.34568, 
                99999999999.9999, 101010101010.10101, 111111111.11111, 1212121212.121212);

            var expected = "111.12 2222.99 33333.56 444444.44 " + Environment.NewLine + 
                           "5555555.32 66666666.01 777777777.10 8888888888.35 " + Environment.NewLine + 
                           "100000000000.00 101010101010.10 111111111.11 1212121212.12 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_4x1()
        {
            var m = Matrix.Create(4, 1,
                1.0, 
                2.0,
                3.0, 
                4.0);

            var expected = " 1.00 " + Environment.NewLine + 
                           " 2.00 " + Environment.NewLine + 
                           " 3.00 " + Environment.NewLine + 
                           " 4.00 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_4x1_RoundedTo2DigitsAfterDecimalPoint()
        {
            var m = Matrix.Create(4, 1,
                1.123, 
                2.567,
                3.987,
                4.543);

            var expected = " 1.12 " + Environment.NewLine + 
                           " 2.57 " + Environment.NewLine + 
                           " 3.99 " + Environment.NewLine + 
                           " 4.54 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_4x1_MoreDigits()
        {
            var m = Matrix.Create(4, 1,
                111.123, 
                2222.567,
                33333.987,
                444444.543);

            var expected = "111.12 " + Environment.NewLine + 
                           "2222.57 " + Environment.NewLine + 
                           "33333.99 " + Environment.NewLine + 
                           "444444.54 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_4x2()
        {
            var m = Matrix.Create(4, 2,
                1.0, 2.0, 
                3.0, 4.0, 
                5.0, 6.0, 
                7.0, 8.0);

            var expected = " 1.00  2.00 " + Environment.NewLine +
                           " 3.00  4.00 " + Environment.NewLine +
                           " 5.00  6.00 " + Environment.NewLine +
                           " 7.00  8.00 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_4x2_RoundedTo2DigitsAfterDecimalPoint()
        {
            var m = Matrix.Create(4, 2,
                1.12345, 2.98765, 
                3.55555, 4.44444, 
                5.32490, 6.009, 
                7.099, 8.34568);

            var expected = " 1.12  2.99 " + Environment.NewLine + 
                           " 3.56  4.44 " + Environment.NewLine + 
                           " 5.32  6.01 " + Environment.NewLine + 
                           " 7.10  8.35 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_4x2_MoreDigits()
        {
            var m = Matrix.Create(4, 2,
                111.12345, 2222.98765, 
                33333.55555, 444444.44444, 
                5555555.32490, 66666666.009, 
                777777777.099, 8888888888.34568);

            var expected = "111.12 2222.99 " + Environment.NewLine + 
                           "33333.56 444444.44 " + Environment.NewLine + 
                           "5555555.32 66666666.01 " + Environment.NewLine + 
                           "777777777.10 8888888888.35 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_4x3()
        {
            var m = Matrix.Create(4, 3,
                1.0, 2.0, 3.0, 
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0,
                10.0, 11.0, 12.0);

            var expected = " 1.00  2.00  3.00 " + Environment.NewLine +
                           " 4.00  5.00  6.00 " + Environment.NewLine +
                           " 7.00  8.00  9.00 " + Environment.NewLine +
                           "10.00 11.00 12.00 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_4x3_RoundedTo2DigitsAfterDecimalPoint()
        {
            var m = Matrix.Create(4, 3,
                1.12345, 2.98765, 3.55555, 
                4.44444, 5.32490, 6.009,
                7.099, 8.34568, 9.9999,
                10.10101, 11.11111, 12.121212);

            var expected = " 1.12  2.99  3.56 " + Environment.NewLine + 
                           " 4.44  5.32  6.01 " + Environment.NewLine + 
                           " 7.10  8.35 10.00 " + Environment.NewLine + 
                           "10.10 11.11 12.12 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_4x3_MoreDigits()
        {
            var m = Matrix.Create(4, 3,
                111.12345, 2222.98765, 33333.55555, 
                444444.44444, 5555555.32490, 66666666.009,
                777777777.099, 8888888888.34568, 99999999999.9999,
                10101010101010.10101, 1111111111111.11111, 1212121212121.121212);

            var expected = "111.12 2222.99 33333.56 " + Environment.NewLine + 
                           "444444.44 5555555.32 66666666.01 " + Environment.NewLine + 
                           "777777777.10 8888888888.35 100000000000.00 " + Environment.NewLine + 
                           "10101010101010.10 1111111111111.11 1212121212121.12 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_4x4()
        {
            var m = Matrix.Create(4, 4,
                1.0, 2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0, 8.0, 
                9.0, 10.0, 11.0, 12.0, 
                13.0, 14.0, 15.0, 16.0);

            var expected = " 1.00  2.00  3.00  4.00 " + Environment.NewLine +
                           " 5.00  6.00  7.00  8.00 " + Environment.NewLine +
                           " 9.00 10.00 11.00 12.00 " + Environment.NewLine +
                           "13.00 14.00 15.00 16.00 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_4x4_RoundedTo2DigitsAfterDecimalPoint()
        {
            var m = Matrix.Create(4, 4,
                1.12345, 2.98765, 3.55555, 4.44444, 
                5.32490, 6.009, 7.099, 8.34568, 
                9.9999, 10.10101, 11.11111, 12.121212,
                13.131313, 14.141414, 15.151515, 16.161616);

            var expected = " 1.12  2.99  3.56  4.44 " + Environment.NewLine + 
                           " 5.32  6.01  7.10  8.35 " + Environment.NewLine + 
                           "10.00 10.10 11.11 12.12 " + Environment.NewLine + 
                           "13.13 14.14 15.15 16.16 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void ToString_4x4_MoreDigits()
        {
            var m = Matrix.Create(4, 4,
                111.12345, 2222.98765, 33333.55555, 444444.44444, 
                5555555.32490, 66666666.009, 777777777.099, 8888888888.34568, 
                99999999999.9999, 1010101010.10101, 1111111111.11111, 1212121212.121212,
                1313131313.131313, 1414141414.141414, 151515151515.151515, 1616161616.161616);

            var expected = "111.12 2222.99 33333.56 444444.44 " + Environment.NewLine + 
                           "5555555.32 66666666.01 777777777.10 8888888888.35 " + Environment.NewLine + 
                           "100000000000.00 1010101010.10 1111111111.11 1212121212.12 " + Environment.NewLine + 
                           "1313131313.13 1414141414.14 151515151515.15 1616161616.16 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        #endregion ToString
        
        #region Parse
        
        [Test]
        public void Parse_Throws_With_EmptyString([Values(
            "",
            "\r\n"
            )] string empty)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Matrix.Parse(empty));
        }
        
        [Test]
        public void Parse_ThrowsMException_With_InvalidString([Values(
            " ",
            "  ",
            "\t",
            "\n",
            "\r",
            "\n\r",
            "1.0 2.5a 3.0\r\n4.0 5.0 6.0",               // number with letter
            "1.0 abc 3.0\r\n4.0 5.0 6.0",                // non numeric
            "1.0 2.0 3.0\r\n4.0 xyz 6.0\r\n7.0 8.0 9.0", // non numeric in 2nd row
            "1.0 2.0 3.0\r\n4.0 5.0 6.0\r\n7.0 8.0 row", // non numeric in 3rd row
            "hello world\r\ntest text",                  // text
            "1.0 @#$ 3.0\r\n4.0 5.0 6.0",                // special symbols
            "1.0 $2,50 3,0\r\n4.0 5.0 6.0",              // special symbols
            "1.0 2..5 3.0\r\n4.0 5.0 6.0",               // double dot
            "1.0 --2.5 3.0\r\n4.0 5.0 6.0",              // double minus
            "1.0 2+3 3.0\r\n4.0 5.0 6.0",                // '+' between digits
            "1.0 2-3 3.0\r\n4.0 5.0 6.0",                // '-' between digits
            "1.0 1/2 3.0\r\n4.0 5.0 6.0",                // '/' between digits
            "1.0 1*2 3.0\r\n4.0 5.0 6.0",                // '*' between digits
            "1.0 0xFF 3.0\r\n4.0 5.0 6.0",               // hex digits
            "1.0 false 3.0\r\n4.0 true 6.0",             // bool not converted to number
            "1.0 2025-01-01 3.0\r\n4.0 5.0 6.0",         // DateTime
            "1.0 π 3.0\r\n4.0 5.0 6.0",                  // Unicode PI
            "1.0\t2.0 3.0\r\n4.0 5.0 6.0"                // Tabs instead of spaces
                )] string invalid)
        {
            var ex = Assert.Throws<MException>(() => Matrix.Parse(invalid));
            Assert.That(ex.Message, Is.EqualTo("Wrong input format!"));
            Assert.That(ex.InnerException, Is.TypeOf<FormatException>());
        }

        private static IEnumerable<TestCaseData> StringValuesWithMissingElementInFirstRow()
        {
            // 2x2
            yield return new TestCaseData(
                    "    2,0" + Environment.NewLine +
                    "3,0 4,0" + Environment.NewLine)
                .SetName("2x2. Missing: [0,0]");
            
            yield return new TestCaseData(
                    "1,0   " + Environment.NewLine +
                    "3,0 4,0" + Environment.NewLine)
                .SetName("2x2. Missing: [0,1]");
            
            // 2x3
            yield return new TestCaseData(
                    "    2,0 3,0" + Environment.NewLine +
                    "4,0 5,0 6,0" + Environment.NewLine)
                .SetName("2x3. Missing: [0,0]");
            
            yield return new TestCaseData(
                    "1,0     3,0" + Environment.NewLine +
                    "4,0 5,0 6,0" + Environment.NewLine)
                .SetName("2x3. Missing: [0,1]");
            
            yield return new TestCaseData(
                    "1,0 2,0    " + Environment.NewLine +
                    "4,0 5,0 6,0" + Environment.NewLine)
                .SetName("2x3. Missing: [0,2]");
            
            // 2x4
            yield return new TestCaseData(
                    "    2,0 3,0 4,0" + Environment.NewLine +
                    "5,0 6,0 7,0 8,0" + Environment.NewLine)
                .SetName("2x4. Missing: [0,0]");
            
            yield return new TestCaseData(
                    "1,0     3,0 4,0" + Environment.NewLine +
                    "5,0 6,0 7,0 8,0" + Environment.NewLine)
                .SetName("2x4. Missing: [0,1]");
            
            yield return new TestCaseData(
                    "1,0 2,0     4,0" + Environment.NewLine +
                    "5,0 6,0 7,0 8,0" + Environment.NewLine)
                .SetName("2x4. Missing: [0,2]");
            
            yield return new TestCaseData(
                    "1,0 2,0 3,0    " + Environment.NewLine +
                    "5,0 6,0 7,0 8,0" + Environment.NewLine)
                .SetName("2x4. Missing: [0,3]");
            
            // 3x2
            yield return new TestCaseData(
                    "    2,0" + Environment.NewLine +
                    "3,0 4,0" + Environment.NewLine +
                    "5,0 6,0" + Environment.NewLine)
                .SetName("3x2. Missing: [0,0]");
            
            yield return new TestCaseData(
                    "1,0    " + Environment.NewLine +
                    "3,0 4,0" + Environment.NewLine +
                    "5,0 6,0" + Environment.NewLine)
                .SetName("3x2. Missing: [0,1]");
            
            // 3x3
            yield return new TestCaseData(
                    "    2,0 3,0" + Environment.NewLine +
                    "4,0 5,0 6,0" + Environment.NewLine +
                    "7,0 8,0 9,0" + Environment.NewLine)
                .SetName("3x3. Missing: [0,0]");
            
            yield return new TestCaseData(
                    "1,0     3,0" + Environment.NewLine +
                    "4,0 5,0 6,0" + Environment.NewLine +
                    "7,0 8,0 9,0" + Environment.NewLine)
                .SetName("3x3. Missing: [0,1]");
            
            yield return new TestCaseData(
                    "1,0 2,0    " + Environment.NewLine +
                    "4,0 5,0 6,0" + Environment.NewLine +
                    "7,0 8,0 9,0" + Environment.NewLine)
                .SetName("3x3. Missing: [0,2]");
            
            // 3x4
            yield return new TestCaseData(
                    "     2,0  3,0  4,0" + Environment.NewLine +
                    "5,0  6,0  7,0  8,0" + Environment.NewLine +
                    "9,0 10,0 11,0 12.0" + Environment.NewLine)
                .SetName("3x4. Missing: [0,0]");
            
            yield return new TestCaseData(
                    "1,0       3,0  4,0" + Environment.NewLine +
                    "5,0  6,0  7,0  8,0" + Environment.NewLine +
                    "9,0 10,0 11,0 12.0" + Environment.NewLine)
                .SetName("3x4. Missing: [0,1]");
            
            yield return new TestCaseData(
                    "1,0  2,0       4,0" + Environment.NewLine +
                    "5,0  6,0  7,0  8,0" + Environment.NewLine +
                    "9,0 10,0 11,0 12.0" + Environment.NewLine)
                .SetName("3x4. Missing: [0,2]");
            
            yield return new TestCaseData(
                    "1,0  2,0  3,0     " + Environment.NewLine +
                    "5,0  6,0  7,0  8,0" + Environment.NewLine +
                    "9,0 10,0 11,0 12.0" + Environment.NewLine)
                .SetName("3x4. Missing: [0,3]");
            
            // 4x2
            yield return new TestCaseData(
                    "    2,0" + Environment.NewLine +
                    "3,0 4,0" + Environment.NewLine +
                    "5,0 6,0" + Environment.NewLine +
                    "7,0 8,0" + Environment.NewLine)
                .SetName("4x2. Missing: [0,0]");
            
            yield return new TestCaseData(
                    "1,0    " + Environment.NewLine +
                    "3,0 4,0" + Environment.NewLine +
                    "5,0 6,0" + Environment.NewLine +
                    "7,0 8,0" + Environment.NewLine)
                .SetName("4x2. Missing: [0,1]");
            
            // 4x3
            yield return new TestCaseData(
                    "      2,0  3,0" + Environment.NewLine +
                    " 4,0  5,0  6,0" + Environment.NewLine +
                    " 7,0  9,0  9,0" + Environment.NewLine +
                    "10,0 11,0 12.0" + Environment.NewLine)
                .SetName("4x3. Missing: [0,0]");
            
            yield return new TestCaseData(
                    " 1,0       3,0" + Environment.NewLine +
                    " 4,0  5,0  6,0" + Environment.NewLine +
                    " 7,0  9,0  9,0" + Environment.NewLine +
                    "10,0 11,0 12.0" + Environment.NewLine)
                .SetName("4x3. Missing: [0,1]");
            
            yield return new TestCaseData(
                    " 1,0  2,0     " + Environment.NewLine +
                    " 4,0  5,0  6,0" + Environment.NewLine +
                    " 7,0  9,0  9,0" + Environment.NewLine +
                    "10,0 11,0 12.0" + Environment.NewLine)
                .SetName("4x3. Missing: [0,2]");
            
            // 4x4
            yield return new TestCaseData(
                    "      2,0  3,0  4,0" + Environment.NewLine +
                    " 5,0  6,0  7,0  8,0" + Environment.NewLine +
                    " 9,0 10,0 11,0 12.0" + Environment.NewLine +
                    "13,0 14,0 15.0 16.0" + Environment.NewLine)
                .SetName("4x4. Missing: [0,0]");
            
            yield return new TestCaseData(
                    " 1,0       3,0  4,0" + Environment.NewLine +
                    " 5,0  6,0  7,0  8,0" + Environment.NewLine +
                    " 9,0 10,0 11,0 12.0" + Environment.NewLine +
                    "13,0 14,0 15.0 16.0" + Environment.NewLine)
                .SetName("4x4. Missing: [0,1]");
            
            yield return new TestCaseData(
                    " 1,0  2,0       4,0" + Environment.NewLine +
                    " 5,0  6,0  7,0  8,0" + Environment.NewLine +
                    " 9,0 10,0 11,0 12.0" + Environment.NewLine +
                    "13,0 14,0 15.0 16.0" + Environment.NewLine)
                .SetName("4x4. Missing: [0,2]");
            
            yield return new TestCaseData(
                    " 1,0  2,0  3,0     " + Environment.NewLine +
                    " 5,0  6,0  7,0  8,0" + Environment.NewLine +
                    " 9,0 10,0 11,0 12.0" + Environment.NewLine +
                    "13,0 14,0 15.0 16.0" + Environment.NewLine)
                .SetName("4x4. Missing: [0,3]");
        }

        [Test]
        [TestCaseSource(nameof(StringValuesWithMissingElementInFirstRow))]
        public void Parse_Throws_With_MissingElement_InFirstRow(string strWithMissingElementInFirstRow)
        {
            Assert.Throws<IndexOutOfRangeException>(() => Matrix.Parse(strWithMissingElementInFirstRow));
        }
        
        [Test]
        public void Parse_With_MissingElement_InLastRow_1x1()
        {
            var str = " 1.00" + Environment.NewLine +
                      "     " + Environment.NewLine;
            
            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(1, 1,
                1.00)));
        }
        
        [Test]
        public void Parse_With_MissingElement_InLastRow_1x2()
        {
            var str = " 1.00 2.00" + Environment.NewLine +
                      "          " + Environment.NewLine;
            
            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(1, 2,
                1.00, 2.00)));
        }
        
        [Test]
        public void Parse_With_MissingElement_InLastRow_1x3()
        {
            var str = " 1.00 2.00 3.00" + Environment.NewLine +
                      "               " + Environment.NewLine;
            
            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(1, 3,
                1.00, 2.00, 3.00)));
        }
        
        [Test]
        public void Parse_With_MissingElements_InLastRow_1x4()
        {
            var str = " 1.00 2.00 3.00 4.00" + Environment.NewLine +
                      "                    " + Environment.NewLine;
            
            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(1, 4,
                1.00, 2.00, 3.00, 4.00)));
        }
        
        [Test]
        public void Parse_With_MissingElement_InLastRow_2x1()
        {
            var str = " 1.00" + Environment.NewLine +
                      " 2.00" + Environment.NewLine +
                      "     " + Environment.NewLine;
            
            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(2, 1,
                1.00,
                2.00)));
        }
        
        [Test]
        public void Parse_With_MissingElement_InLastRow_2x2()
        {
            var str = " 1.00 2.00" + Environment.NewLine +
                      " 3.00     " + Environment.NewLine;
            
            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(2, 2,
                1.00, 2.00,
                3.00, 0.00)));
        }
        
        [Test]
        public void Parse_With_MissingElement_InLastRow_2x3()
        {
            var str = " 1.00 2.00 3.00" + Environment.NewLine +
                      "      4.00     " + Environment.NewLine;
            
            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(2, 3,
                1.00, 2.00, 3.00,
                4.00, 0.00, 0.00)));
        }
        
        [Test]
        public void Parse_With_MissingElement_InLastRow_2x4()
        {
            var str = " 1.00 2.00 3.00 4.00" + Environment.NewLine +
                      "           5.00     " + Environment.NewLine;
            
            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(2, 4,
                1.00, 2.00, 3.00, 4.00,
                5.00, 0.00, 0.00, 0.00)));
        }
        
        [Test]
        public void Parse_With_MissingElement_InLastRow_3x1()
        {
            var str = " 1.00" + Environment.NewLine +
                      " 2.00" + Environment.NewLine +
                      " 3.00" + Environment.NewLine +
                      "     " + Environment.NewLine;
            
            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(3, 1,
                1.00,
                2.00,
                3.00)));
        }
        
        [Test]
        public void Parse_With_MissingElement_InLastRow_3x2()
        {
            var str = " 1.00 2.00" + Environment.NewLine +
                      " 3.00 4.00" + Environment.NewLine +
                      "      5.00" + Environment.NewLine;
            
            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(3, 2,
                1.00, 2.00, 
                3.00, 4.00, 
                5.00, 0.00)));
        }
        
        [Test]
        public void Parse_With_MissingElement_InLastRow_3x3()
        {
            var str = " 1.00 2.00 3.00" + Environment.NewLine +
                      " 4.00 5.00 6.00" + Environment.NewLine +
                      "           7.00" + Environment.NewLine;
            
            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(3, 3,
                1.00, 2.00, 3.00, 
                4.00, 5.00, 6.00,
                7.00, 0.00, 0.00)));
        }
        
        [Test]
        public void Parse_With_MissingElement_InLastRow_3x4()
        {
            var str = " 1.00 2.00 3.00 4.00" + Environment.NewLine +
                      " 5.00 6.00 7.00 8.00" + Environment.NewLine +
                      "                9.00" + Environment.NewLine;
            
            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(3, 4,
                1.00, 2.00, 3.00, 4.00, 
                5.00, 6.00, 7.00, 8.00, 
                9.00, 0.00, 0.00, 0.00)));
        }
        
        [Test]
        public void Parse_With_MissingElement_InLastRow_4x1()
        {
            var str = " 1.00" + Environment.NewLine +
                      " 2.00" + Environment.NewLine +
                      " 3.00" + Environment.NewLine +
                      " 4.00" + Environment.NewLine +
                      "     " + Environment.NewLine;
            
            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(4, 1,
                1.00,
                2.00,
                3.00,
                4.00)));
        }
        
        [Test]
        public void Parse_With_MissingElement_InLastRow_4x2()
        {
            var str = " 1.00 2.00" + Environment.NewLine +
                      " 3.00 4.00" + Environment.NewLine +
                      " 5.00 6.00" + Environment.NewLine +
                      " 7.00     " + Environment.NewLine;
            
            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(4, 2,
                1.00, 2.00,
                3.00, 4.00,
                5.00, 6.00,
                7.00, 0.00)));
        }
        
        [Test]
        public void Parse_With_MissingElement_InLastRow_4x3()
        {
            var str = " 1.00 2.00 3.00" + Environment.NewLine +
                      " 4.00 5.00 6.00" + Environment.NewLine +
                      " 7.00 8.00 9.00" + Environment.NewLine +
                      "     10.00     " + Environment.NewLine;
            
            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(4, 3,
                1.00, 2.00, 3.00, 
                4.00, 5.00, 6.00,
                7.00, 8.00, 9.00,
                10.00, 0.00, 0.00)));
        }
        
        [Test]
        public void Parse_With_MissingElement_InLastRow_4x4()
        {
            var str = " 1.00  2.00  3.00  4.00" + Environment.NewLine +
                      " 5.00  6.00  7.00  8.00" + Environment.NewLine +
                      " 9.00 10.00 11.00 12.00" + Environment.NewLine +
                      "                  13.00" + Environment.NewLine;
            
            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(4, 4,
                1.00, 2.00, 3.00, 4.00, 
                5.00, 6.00, 7.00, 8.00, 
                9.00, 10.00, 11.00, 12.00,
                13.00, 0.00, 0.00, 0.00)));
        }
        
        [Test]
        public void Parse_1x1()
        {
            var str = " 1.00 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(1, 1,
                1.0)));
        }
        
        [Test]
        public void Parse_1x1_MoreDigitsAfterDecimalPoint()
        {
            var str = " 1.5678 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(1, 1,
                1.5678)));
        }
        
        [Test]
        public void Parse_1x1_MoreDigits()
        {
            var m = Matrix.Create(1, 1,
                1234.5678);

            var expected = "1234.57 " + Environment.NewLine;
            Assert.That(m.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void Parse_1x2()
        {
            var str = " 1.00  2.00 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(1, 2,
                1.0, 2.0)));
        }
        
        [Test]
        public void Parse_1x2_MoreDigitsAfterDecimalPoint()
        {
            var str = " 1.123  2.567 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(1, 2,
                1.123, 2.567)));
        }
        
        [Test]
        public void Parse_1x2_MoreDigits()
        {
            var str = " 1234.123 23456.567 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(1, 2,
                1234.123, 23456.567)));
        }
        
        [Test]
        public void Parse_1x3()
        {
            var str = " 1.0 2.0 3.0" + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(1, 3,
                1.0, 2.0, 3.0)));
        }
        
        [Test]
        public void Parse_1x3_MoreDigitsAfterDecimalPoint()
        {
            var str = " 1.123 2.567 3.987" + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(1, 3,
                1.123, 2.567, 3.987)));
        }
        
        [Test]
        public void Parse_1x3_MoreDigits()
        {
            var str = " 123.123 2345.567 34567.987" + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(1, 3,
                123.123, 2345.567, 34567.987)));
        }
        
        [Test]
        public void Parse_1x4()
        {
            var str = " 1.0 2.0 3.0 4.0" + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(1, 4,
                1.0, 2.0, 3.0, 4.0)));
        }
        
        [Test]
        public void Parse_1x4_MoreDigitsAfterDecimalPoint()
        {
            var str = " 1.123 2.567 3.987 4.543" + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(1, 4,
                1.123, 2.567, 3.987, 4.543)));
        }
        
        [Test]
        public void Parse_1x4_MoreDigits()
        {
            var str = " 123.123 2345.567 34567.987 456789.543" + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(1, 4,
                123.123, 2345.567, 34567.987, 456789.543)));
        }
        
        [Test]
        public void Parse_2x1()
        {
            var str = " 1.00 " + Environment.NewLine +
                      " 2.00 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(2, 1,
                1.0, 
                2.0)));
        }
        
        [Test]
        public void Parse_2x1_MoreDigitsAfterDecimalPoint()
        {
            var str = " 1.123 " + Environment.NewLine +
                      " 2.567 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(2, 1,
                1.123, 
                2.567)));
        }
        
        [Test]
        public void Parse_2x1_MoreDigits()
        {
            var str = " 123.123 " + Environment.NewLine +
                      " 2345.567 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(2, 1,
                123.123, 
                2345.567)));
        }
        
        [Test]
        public void Parse_2x2()
        {
            var str = " 1.00  2.00 " + Environment.NewLine +
                      " 3.00  4.00 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(2, 2,
                1.0, 2.0,
                3.0, 4.0)));
        }
        
        [Test]
        public void Parse_2x2_MoreDigitsAfterDecimalPoint()
        {
            var str = " 1.12345 2.98765 " + Environment.NewLine +
                      " 3.55555 4.44444 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(2, 2,
                1.12345, 2.98765,
                3.55555, 4.44444)));
        }
        
        [Test]
        public void Parse_2x2_MoreDigits()
        {
            var str = " 123.12345 2345.98765 " + Environment.NewLine +
                      " 34567.55555 456789.44444 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(2, 2,
                123.12345, 2345.98765,
                34567.55555, 456789.44444)));
        }
        
        [Test]
        public void Parse_2x3()
        {
            var str = " 1.00  2.00  3.00 " + Environment.NewLine +
                      " 4.00  5.00  6.00 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(2, 3,
                1.0, 2.0, 3.0, 
                4.0, 5.0, 6.0)));
        }
        
        [Test]
        public void Parse_2x3_MoreDigitsAfterDecimalPoint()
        {
            var str = " 1.12345 2.98765 3.55555 " + Environment.NewLine +
                      " 4.44444 5.32490 6.009 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(2, 3,
                1.12345, 2.98765, 3.55555, 
                4.44444, 5.32490, 6.009)));
        }
        
        [Test]
        public void Parse_2x3_MoreDigits()
        {
            var str = " 111.12345 2222.98765 33333.55555 " + Environment.NewLine +
                      " 444444.44444 5555555.32490 66666666.009 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(2, 3,
                111.12345, 2222.98765, 33333.55555, 
                444444.44444, 5555555.32490, 66666666.009)));
        }
        
        [Test]
        public void Parse_2x4()
        {
            var str = " 1.00  2.00  3.00  4.00 " + Environment.NewLine +
                      " 5.00  6.00  7.00  8.00 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(2, 4,
                1.0, 2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0, 8.0)));
        }
        
        [Test]
        public void Parse_2x4_MoreDigitsAfterDecimalPoint()
        {
            var str = " 1.12345 2.98765 3.55555 4.44444 " + Environment.NewLine +
                      " 5.32490 6.009 7.099 8.34568 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(2, 4,
                1.12345, 2.98765, 3.55555, 4.44444, 
                5.32490, 6.009, 7.099, 8.34568)));
        }
        
        [Test]
        public void Parse_2x4_MoreDigits()
        {
            var str = " 111.12345 2222.98765 33333.55555 444444.44444 " + Environment.NewLine +
                      " 5555555.32490 66666666.009 777777777.099 888888888.34568 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(2, 4,
                111.12345, 2222.98765, 33333.55555, 444444.44444, 
                5555555.32490, 66666666.009, 777777777.099, 888888888.34568)));
        }
        
        [Test]
        public void Parse_3x1()
        {
            var str = " 1.00 " + Environment.NewLine +
                      " 2.00 " + Environment.NewLine +
                      " 3.00 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(3, 1,
                1.0, 
                2.0, 
                3.0)));
        }
        
        [Test]
        public void Parse_3x1_MoreDigitsAfterDecimalPoint()
        {
            var str = " 1.123 " + Environment.NewLine +
                      " 2.567 " + Environment.NewLine +
                      " 3.987 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(3, 1,
                1.123, 
                2.567, 
                3.987)));
        }
        
        [Test]
        public void Parse_3x1_MoreDigits()
        {
            var str = " 111.123 " + Environment.NewLine +
                      " 2222.567 " + Environment.NewLine +
                      " 33333.987 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(3, 1,
                111.123, 
                2222.567, 
                33333.987)));
        }
        
        [Test]
        public void Parse_3x2()
        {
            var str = " 1.00  2.00 " + Environment.NewLine +
                      " 3.00  4.00 " + Environment.NewLine +
                      " 5.00  6.00 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(3, 2,
                1.0, 2.0, 
                3.0, 4.0, 
                5.0, 6.0)));
        }
        
        [Test]
        public void Parse_3x2_MoreDigitsAfterDecimalPoint()
        {
            var str = " 1.12345  2.98765 " + Environment.NewLine +
                      " 3.55555  4.44444 " + Environment.NewLine +
                      " 5.32490  6.009 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(3, 2,
                1.12345, 2.98765, 
                3.55555, 4.44444, 
                5.32490, 6.009)));
        }
        
        [Test]
        public void Parse_3x2_MoreDigits()
        {
            var str = " 111.12345 2222.98765 " + Environment.NewLine +
                      " 33333.55555 444444.44444 " + Environment.NewLine +
                      " 5555555.32490 66666666.009 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(3, 2,
                111.12345, 2222.98765, 
                33333.55555, 444444.44444, 
                5555555.32490, 66666666.009)));
        }
        
        [Test]
        public void Parse_3x3()
        {
            var str = " 1.00  2.00  3.00 " + Environment.NewLine +
                      " 4.00  5.00  6.00 " + Environment.NewLine +
                      " 7.00  8.00  9.00 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(3, 3,
                1.0, 2.0, 3.0, 
                4.0, 5.0, 6.0, 
                7.0, 8.0, 9.0)));
        }
        
        [Test]
        public void Parse_3x3_MoreDigitsAfterDecimalPoint()
        {
            var str = " 1.12345 2.98765 3.55555 " + Environment.NewLine +
                      " 4.44444 5.32490 6.009 " + Environment.NewLine +
                      " 7.099 8.34568 9.9999 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(3, 3,
                1.12345, 2.98765, 3.55555, 
                4.44444, 5.32490, 6.009, 
                7.099, 8.34568, 9.9999)));
        }
        
        [Test]
        public void Parse_3x3_MoreDigits()
        {
            var str = " 111.12345 2222.98765 33333.55555 " + Environment.NewLine +
                      " 444444.44444 5555555.32490 66666666.009 " + Environment.NewLine +
                      " 777777777.099 88888888888.34568 9999999999.9999 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(3, 3,
                111.12345, 2222.98765, 33333.55555, 
                444444.44444, 5555555.32490, 66666666.009, 
                777777777.099, 88888888888.34568, 9999999999.9999)));
        }
        
        [Test]
        public void Parse_3x4()
        {
            var str = " 1.00  2.00  3.00  4.00 " + Environment.NewLine +
                      " 5.00  6.00  7.00  8.00 " + Environment.NewLine +
                      " 9.00 10.00 11.00 12.00 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(3, 4,
                1.0, 2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0, 8.0, 
                9.0, 10.0, 11.0, 12.0)));
        }
        
        [Test]
        public void Parse_3x4_MoreDigitsAfterDecimalPoint()
        {
            var str = " 1.12345 2.98765 3.55555 4.44444 " + Environment.NewLine +
                      " 5.32490 6.009 7.099 8.34568 " + Environment.NewLine +
                      " 9.9999 10.10101 11.11111 12.121212 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(3, 4,
                1.12345, 2.98765, 3.55555, 4.44444, 
                5.32490, 6.009, 7.099, 8.34568, 
                9.9999, 10.10101, 11.11111, 12.121212)));
        }
        
        [Test]
        public void Parse_3x4_MoreDigits()
        {
            var str = " 111.12345 2222.98765 33333.55555 444444.44444 " + Environment.NewLine +
                      " 5555555.32490 66666666.009 777777777.099 8888888888.34568 " + Environment.NewLine +
                      " 99999999999.9999 101010101010.10101 111111111.11111 1212121212.121212 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(3, 4,
                111.12345, 2222.98765, 33333.55555, 444444.44444, 
                5555555.32490, 66666666.009, 777777777.099, 8888888888.34568, 
                99999999999.9999, 101010101010.10101, 111111111.11111, 1212121212.121212)));
        }
        
        [Test]
        public void Parse_4x1()
        {
            var str = " 1.00 " + Environment.NewLine + 
                      " 2.00 " + Environment.NewLine + 
                      " 3.00 " + Environment.NewLine + 
                      " 4.00 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(4, 1,
                1.0, 
                2.0,
                3.0, 
                4.0)));
        }
        
        [Test]
        public void Parse_4x1_MoreDigitsAfterDecimalPoint()
        {
            var str = " 1.123 " + Environment.NewLine + 
                      " 2.567 " + Environment.NewLine + 
                      " 3.987 " + Environment.NewLine + 
                      " 4.543 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(4, 1,
                1.123, 
                2.567,
                3.987,
                4.543)));
        }
        
        [Test]
        public void Parse_4x1_MoreDigits()
        {
            var str = " 111.123 " + Environment.NewLine + 
                      " 2222.567 " + Environment.NewLine + 
                      " 33333.987 " + Environment.NewLine + 
                      " 444444.543 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(4, 1,
                111.123, 
                2222.567,
                33333.987,
                444444.543)));
        }
        
        [Test]
        public void Parse_4x2()
        {
            var str = " 1.00  2.00 " + Environment.NewLine +
                      " 3.00  4.00 " + Environment.NewLine +
                      " 5.00  6.00 " + Environment.NewLine +
                      " 7.00  8.00 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(4, 2,
                1.0, 2.0, 
                3.0, 4.0, 
                5.0, 6.0, 
                7.0, 8.0)));
        }
        
        [Test]
        public void Parse_4x2_MoreDigitsAfterDecimalPoint()
        {
            var str = " 1.12345 2.98765 " + Environment.NewLine +
                      " 3.55555 4.44444 " + Environment.NewLine +
                      " 5.32490 6.009 " + Environment.NewLine +
                      " 7.099 8.34568 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(4, 2,
                1.12345, 2.98765, 
                3.55555, 4.44444, 
                5.32490, 6.009, 
                7.099, 8.34568)));
        }
        
        [Test]
        public void Parse_4x2_MoreDigits()
        {
            var str = " 111.12345 2222.98765 " + Environment.NewLine +
                      " 33333.55555 444444.44444 " + Environment.NewLine +
                      " 5555555.32490 66666666.009 " + Environment.NewLine +
                      " 777777777.099 8888888888.34568 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(4, 2,
                111.12345, 2222.98765, 
                33333.55555, 444444.44444, 
                5555555.32490, 66666666.009, 
                777777777.099, 8888888888.34568)));
        }
        
        [Test]
        public void Parse_4x3()
        {
            var str = " 1.00  2.00  3.00 " + Environment.NewLine +
                      " 4.00  5.00  6.00 " + Environment.NewLine +
                      " 7.00  8.00  9.00 " + Environment.NewLine +
                      "10.00 11.00 12.00 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(4, 3,
                1.0, 2.0, 3.0, 
                4.0, 5.0, 6.0,
                7.0, 8.0, 9.0,
                10.0, 11.0, 12.0)));
        }
        
        [Test]
        public void Parse_4x3_MoreDigitsAfterDecimalPoint()
        {
            var str = "1.12345 2.98765 3.55555 " + Environment.NewLine +
                      "4.44444 5.32490 6.009 " + Environment.NewLine +
                      "7.099 8.34568 9.9999 " + Environment.NewLine +
                      "10.10101 11.11111 12.121212 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(4, 3,
                1.12345, 2.98765, 3.55555, 
                4.44444, 5.32490, 6.009,
                7.099, 8.34568, 9.9999,
                10.10101, 11.11111, 12.121212)));
        }
        
        [Test]
        public void Parse_4x3_MoreDigits()
        {
            var str = " 111.12345 2222.98765 33333.55555 " + Environment.NewLine +
                      " 444444.44444 5555555.32490 66666666.009 " + Environment.NewLine +
                      " 777777777.099 8888888888.34568 99999999999.9999 " + Environment.NewLine +
                      " 10101010101010.10101 1111111111111.11111 1212121212121.121212 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(4, 3,
                111.12345, 2222.98765, 33333.55555, 
                444444.44444, 5555555.32490, 66666666.009,
                777777777.099, 8888888888.34568, 99999999999.9999,
                10101010101010.10101, 1111111111111.11111, 1212121212121.121212)));
        }
        
        [Test]
        public void Parse_4x4()
        {
            var str = " 1.00  2.00  3.00  4.00 " + Environment.NewLine +
                      " 5.00  6.00  7.00  8.00 " + Environment.NewLine +
                      " 9.00 10.00 11.00 12.00 " + Environment.NewLine +
                      "13.00 14.00 15.00 16.00 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(4, 4,
                1.0, 2.0, 3.0, 4.0, 
                5.0, 6.0, 7.0, 8.0, 
                9.0, 10.0, 11.0, 12.0, 
                13.0, 14.0, 15.0, 16.0)));
        }
        
        [Test]
        public void Parse_4x4_MoreDigitsAfterDecimalPoint()
        {
            var str = "1.12345 2.98765 3.55555 4.44444 " + Environment.NewLine +
                      "5.32490 6.009 7.099 8.34568 " + Environment.NewLine +
                      "9.9999 10.10101 11.11111 12.121212 " + Environment.NewLine +
                      "13.131313 14.141414 15.151515 16.161616 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(4, 4,
                1.12345, 2.98765, 3.55555, 4.44444, 
                5.32490, 6.009, 7.099, 8.34568, 
                9.9999, 10.10101, 11.11111, 12.121212,
                13.131313, 14.141414, 15.151515, 16.161616)));
        }
        
        [Test]
        public void Parse_4x4_MoreDigits()
        {
            var str = "111.12345 2222.98765 33333.55555 444444.44444 " + Environment.NewLine +
                      "5555555.32490 66666666.009 777777777.099 8888888888.34568 " + Environment.NewLine +
                      "99999999999.9999 1010101010.10101 1111111111.11111 1212121212.121212 " + Environment.NewLine +
                      "1313131313.131313 1414141414.141414 151515151515.151515 1616161616.161616 " + Environment.NewLine;

            Assert.That(Matrix.Parse(str), Is.EqualTo(Matrix.Create(4, 4,
                111.12345, 2222.98765, 33333.55555, 444444.44444, 
                5555555.32490, 66666666.009, 777777777.099, 8888888888.34568, 
                99999999999.9999, 1010101010.10101, 1111111111.11111, 1212121212.121212,
                1313131313.131313, 1414141414.141414, 151515151515.151515, 1616161616.161616)));
        }
        
        #endregion Parse
        
        #region Matrix2x3 helper class
        
        [Test]
        public void Matrix2x3_class_SameAs_Matrix_2x3()
        {
            var matrix2x3 = new Matrix2x3();
            matrix2x3[0,0] = 0.0;
            matrix2x3[0,1] = 1.0;
            matrix2x3[0,2] = 2.0;
            matrix2x3[1,0] = 3.0;
            matrix2x3[1,1] = 4.0;
            matrix2x3[1,2] = 5.0;

            Matrix expected = Matrix.Create(2, 3,
                0.0, 1.0,
                2.0, 3.0,
                4.0, 5.0);
            
            Assert.That(matrix2x3.cols, Is.EqualTo(expected.cols));
            Assert.That(matrix2x3.rows, Is.EqualTo(expected.rows));
            Assert.That(matrix2x3, Is.EqualTo(expected));
            Assert.That(matrix2x3.GetElements().ToArray(), Is.EqualTo(expected.GetElements().ToArray()).AsCollection.Within(G.Precision));
        }
        
        #endregion Matrix2x3 helper class
        
        #region Vector3 helper class
        
        [Test]
        public void Vector3_class_SameAs_Matrix_3x1()
        {
            var vector3 = new Vector3();
            vector3[0, 0] = 0.0;
            vector3[1, 0] = 1.0;
            vector3[2, 0] = 2.0;

            Matrix expected = Matrix.Create(3, 1,
                0.0, 
                1.0,
                2.0);
            
            Assert.That(vector3.cols, Is.EqualTo(expected.cols));
            Assert.That(vector3.rows, Is.EqualTo(expected.rows));
            Assert.That(vector3, Is.EqualTo(expected));
            Assert.That(vector3.GetElements().ToArray(), Is.EqualTo(expected.GetElements().ToArray()).AsCollection.Within(G.Precision));
        }
        
        #endregion Vector3 helper class
    }
}