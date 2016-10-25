using System;

namespace DefLab.common.math
{

    public class Vector2D
    {
        public Vector2D(Coordinate2D c)
        {
            X = c.X;
            Y = c.Y;
            A = new Coordinate2D(0, 0);
        }

        public Vector2D(Coordinate2D startCoord, Coordinate2D endCoord)
        {
            X = endCoord.X - startCoord.X;
            Y = endCoord.Y - startCoord.Y;
            A = startCoord;
        }

        public double X { get; private set; }
        public double Y { get; private set; }
        public Coordinate2D A { get; private set; }

        public Coordinate2D B
        {
            get { return getEndPoint(); }
        }

        /// <summary>
        ///     Overrided to ignore Z coordinate (because it contains NaN)
        /// </summary>
        public double Length
        {
            get { return Math.Sqrt(X * X + Y * Y); }
            set
            {
                if (value > 0)
                {
                    double k = value / Length;
                    X *= k;
                    Y *= k;
                }
            }
        }

        private Coordinate2D getEndPoint()
        {
            return new Coordinate2D(X + A.X, Y + A.Y);
        }

        public bool isCoDirectional(Vector2D vector)
        {
            var xSameSign = X * vector.X >= 0;
            var ySameSign = Y * vector.Y >= 0;

            return xSameSign && ySameSign;
        }

        /// <summary>
        ///     Полученный вектор только меняет направление, но сохраняет пространственное положение.
        /// </summary>
        /// <returns></returns>
        public Vector2D getInverted()
        {
            return new Vector2D(B, A);
        }

        /// <summary>
        ///     rotated 90 degrees CW
        /// </summary>
        /// <returns></returns>
        public Vector2D getPerpendicularVector()
        {
            return getPerpendicularVector(RotateDirection.rdClockWise);
        }

        /// <summary>
        ///     CW or CCW rotation
        /// </summary>
        /// <param name="rotateDirection">direction enum</param>
        /// <returns></returns>
        public Vector2D getPerpendicularVector(RotateDirection rotateDirection)
        {
            switch (rotateDirection)
            {
                case RotateDirection.rdClockWise:
                    return fastRotate90Clockwise(); // cwPerpendicularVector;
                case RotateDirection.rdCounterClockWise:
                    return fastRotate90CounterClockwise(); // ccwPerpendicularVector;
                default:
                    throw new NotImplementedException("RotateDirection not supported: " + rotateDirection);
            }
        }

        public Vector2D copy()
        {
            var startCopy = new Coordinate2D(A.X, A.Y);
            return new Vector2D(startCopy, B);
        }

        /// <summary>
        ///     270 degrees rotation
        /// </summary>
        /// <returns></returns>
        public Vector2D fastRotate90Clockwise()
        {
            return fastRotate(new double[,]
            {
                {0, 1},
                {-1, 0}
            });
        }

        /// <summary>
        ///     90 degrees rotation
        /// </summary>
        /// <returns></returns>
        public Vector2D fastRotate90CounterClockwise()
        {
            return fastRotate(new double[,]
            {
                {0, -1},
                {1, 0}
            });
        }

        /// <summary>
        ///     Поворот на произвольный угол по часовой стрелке
        /// </summary>
        /// <param name="angle">угол поворота в радианах</param>
        /// <returns></returns>
        public Vector2D fastRotateAngleClockwise(double angle)
        {
            var sinA = Math.Sin(angle);
            var cosA = Math.Cos(angle);
            return fastRotate(new[,]
            {
                {cosA, -sinA},
                {sinA, cosA}
            });
        }

        /// <summary>
        ///     180 degrees rotation around A point (vector position changed)
        /// </summary>
        /// <returns></returns>
        public Vector2D getOppositeVector()
        {
            return fastRotate(new double[,]
            {
                {-1, 0},
                {0, -1}
            });
        }

        /// <summary>
        ///     alpha degrees rotation
        /// </summary>
        /// <returns></returns>
        private Vector2D fastRotate(double[,] rotationMatrix)
        {
            var newX = rotationMatrix[0, 0] * X + rotationMatrix[0, 1] * Y;
            var newY = rotationMatrix[1, 0] * X + rotationMatrix[1, 1] * Y;

            var newB = new Coordinate2D(newX + A.X, newY + A.Y);

            return new Vector2D(A, newB);
        }

        public double calcZofVectorProduction(Vector2D anotherVector)
        {
            return X * anotherVector.Y - anotherVector.X * Y;
        }

        /// <summary>
        ///     рассчитывает угол между векторами в градусах (0..360)
        /// </summary>
        /// <param name="anotherVector"></param>
        /// <returns></returns>
        public double calcAngleDeg(Vector2D anotherVector)
        {
            var cos = calcCosinus(anotherVector);

            //just in case
            if (cos > 1)
            {
                cos = 1;
            }

            return MathNet.Numerics.Trig.RadianToDegree(Math.Acos(cos));
        }

        /// <summary>
        ///     рассчитывает угол между векторами в радианах
        /// </summary>
        /// <param name="anotherVector"></param>
        /// <returns></returns>
        public double calcAngleRad(Vector2D anotherVector)
        {
            var cos = calcCosinus(anotherVector);

            //just in case
            if (cos > 1)
            {
                cos = 1;
            }

            return Math.Acos(cos);
        }

        /// <summary>
        ///     рассчитывает угол между векторами в радианах с учетом знака.
        ///     Направление вращение между векторами по часовой стрелке - дает положительный угол. Иначе - отрицательный.
        /// </summary>
        /// <param name="anotherVector"></param>
        /// <returns></returns>
        public double calcAngleRadSign(Vector2D anotherVector)
        {
            var cos = calcCosinus(anotherVector);

            //just in case
            if (cos > 1)
            {
                cos = 1;
            }

            return Math.Acos(cos);
        }

        /// <summary>
        ///     рассчитывает косинус угла между векторами
        /// </summary>
        /// <param name="anotherVector"></param>
        /// <returns></returns>
        public double calcCosinus(Vector2D anotherVector)
        {
            var scalarProduct = calcScalarProduct(anotherVector);
            var moduleProduct = Length * anotherVector.Length;

            if (Math.Abs(moduleProduct - 0) < 0.000001)
            {
                return 0;
            }

            return scalarProduct / moduleProduct;
        }

        //        /// <summary>
        //        ///     рассчитывает синус угла между векторами
        //        /// </summary>
        //        /// <param name="anotherVector"></param>
        //        /// <returns></returns>
        //        public double calcSinus(Vector2D anotherVector) {
        //            var scalarProduct = calcScalarProduct(anotherVector);
        //            var moduleProduct = Length*anotherVector.Length;
        //
        //            if (Math.Abs(moduleProduct - 0) < 0.000001) {
        //                return 0;
        //            }
        //
        //            return moduleProduct / scalarProduct;
        //        }

        public double calcScalarProduct(Vector2D anotherVector)
        {
            return X * anotherVector.X + Y * anotherVector.Y;
        }
    }

    public enum RotateDirection
    {
        /// <summary>
        ///     По часовой стрелке
        /// </summary>
        rdClockWise,

        /// <summary>
        ///     Против часовой стрелки
        /// </summary>
        rdCounterClockWise
    }
}