using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DefLab.common.kriging.spatial {
    public class SpatialContinuity : ISpatialContinuityData {
        private readonly int _npoints;
        private readonly List<GridPoint> _gridPoints;
        private Lag _lag;
        private Stat[,] _spatConti;
        private Stat[,] _spatDist;
        private Covariance[,] _spatContiCov;
        private Stat _semivariance;
        private Stat _distance;
        private readonly Stat _covariance;
        private SortedList<int, double> _distancesA;
        private SortedList<int, double> _semivarianceA;
        private SortedList<int, double> _covarianceA;
        private bool _calculated;

        public event EventHandler ChangedLagParameter;

        protected virtual void onChangedLagParameter(EventArgs e) {
            var h = ChangedLagParameter;
            if (h != null) {
                h(this, e);
            }
            _calculated = false;
        }

        #region properties

        public Stat[,] DistancesMeanOfGrid
        {
            get { return _spatDist; }
        }

        public Covariance[,] CovarianceOfGrid
        {
            get { return _spatContiCov; }
        }

        public Stat[,] SemivarianceOfGrid
        {
            get { return _spatConti; }
        }


        public Stat SemivarianceStatistic
        {
            get { return _semivariance; }
        }

        public Stat DistanceStatistic
        {
            get { return _distance; }
        }


        public Stat CovatiranceStatistic
        {
            get { return _covariance; }
        }

        public Lag LagValue
        {
            get { return _lag; }
            set
            {
                if (!LagSize.Equals(value.lagSize) ||
                    NumberOfLags != value.numLags ||
                    !_lag.tolerance.Equals(value.tolerance)) {
                    _lag = value;
                    grid();
                    calculateDistancesLag();
                    _calculated = false;
                    onChangedLagParameter(EventArgs.Empty);
                }
            }
        }

        public int NumberOfLags
        {
            set
            {
                NumberOfLags = value;
                grid();
                calculateDistancesLag();
                onChangedLagParameter(EventArgs.Empty);
            }
            get { return _lag.numLags; }
        }

        public double LagSize
        {
            set
            {
                LagSize = value;
                grid();
                calculateDistancesLag();
                onChangedLagParameter(EventArgs.Empty);
            }
            get { return _lag.lagSize; }
        }

        public double[] Semivariances
        {
            get
            {
                if (_calculated) {
                    return _semivarianceA.Values.ToArray();
                }
                calculateListValues();
                return _semivarianceA.Values.ToArray();
            }
        }

        public double[] Distances
        {
            get
            {
                if (_calculated) {
                    return _distancesA.Values.ToArray();
                }
                calculateListValues();
                return _distancesA.Values.ToArray();
            }
        }

        public GridPoint[] SemivariancePoints
        {
            get
            {
                var distances = Distances;
                var semivariances = Semivariances;
                var size = Math.Min(distances.Length, semivariances.Length);
                if (size == 0) {
                    return null;
                }
                var result = new GridPoint[size];
                for (int i = 0; i < size; i++) {
                    result[i] = new GridPoint(distances[i], semivariances[i]);
                }
                return result;
            }
        }

        public double[] Covariances
        {
            get
            {
                if (_calculated) {
                    return _covarianceA.Values.ToArray();
                }
                calculateListValues();
                return _covarianceA.Values.ToArray();
            }
        }

        #endregion

        public SpatialContinuity(Lag lag, List<GridPoint> gridPoints) {
            _semivariance = new Stat(true);
            _distance = new Stat(true);
            _covariance = new Stat(true);

            _gridPoints = gridPoints;
            _npoints = gridPoints.Count;
            _lag = lag;
            _gridPoints = gridPoints;
            grid();
            calculateDistances();
            calculateDistancesLag();
            _calculated = false;
        }

        public SpatialContinuity(List<GridPoint> gridPoints) {
            _semivariance = new Stat(true);
            _distance = new Stat(true);
            _covariance = new Stat(true);

            _gridPoints = gridPoints;
            _npoints = gridPoints.Count;
            calculateDistances();
            _lag = new Lag(12, _distance.Max*0.7/12, 0.5);
            grid();
            calculateDistancesLag();
            _calculated = false;
        }

        public SpatialContinuity(List<GridPoint> gridPoints, CancellationToken cancellationToken) {
            _semivariance = new Stat(true);
            _distance = new Stat(true);
            _covariance = new Stat(true);

            _gridPoints = gridPoints;
            _npoints = gridPoints.Count;
            calculateDistances();
            _lag = new Lag(12, _distance.Max*0.7/12, 0.5);
            grid();
            calculateDistancesLag(cancellationToken);
            _calculated = false;
        }

        private void grid() {
            if (_lag == null) {
                return;
            }

            _spatConti = new Stat[_lag.numLags*2, _lag.numLags*2];
            _spatContiCov = new Covariance[_lag.numLags*2, _lag.numLags*2];
            _spatDist = new Stat[_lag.numLags*2, _lag.numLags*2];
            for (int row = 0; row < _lag.numLags*2; row++) {
                for (int col = 0; col < _lag.numLags*2; col++) {
                    _spatConti[row, col] = new Stat(true);
                    _spatContiCov[row, col] = new Covariance(true);
                    _spatDist[row, col] = new Stat(true);
                }
            }
        }

        /*
        public IFeatureSet InfluenceZone(string majorR, string minorR, string az)
        {  
            double value=1;
            try{
               value=Convert.ToDouble(majorR);
               if (!minorR.Equals(""))
               { 
                   return  InfluenceZone(value,Convert.ToDouble(minorR), Utils.ToRad(Convert.ToDouble(az)));
               }
            } catch
            {
            
            }


        return InfluenceZone(_lag.numLags * _lag.lagSize,_lag.numLags * _lag.lagSize,0);
        
        }

        public IFeatureSet InfluenceZone(double majorR, double minorR, double az)
        {
            if (_lag == null)
                return null;

            FeatureSet border = new FeatureSet(DotSpatial.Topology.FeatureType.Polygon);
            border.Projection = KnownCoordinateSystems.Projected.UtmWgs1984.WGS1984UTMZone10N;
            double size = _lag.numLags * 2 * _lag.lagSize;

            Coordinate center = new Coordinate(500000 + (size / 2), size / 2);

            //Coordinate[] coorPol = new Coordinate[73];
            //int j=0;
            //for (int i = 0; i < 360; i += 5)
            //{ 
            
            //coorPol[j]=AzimuthDist(center, i*Math.PI/180.0,lag.numLags  * lag.lagSize);
            //    j++;
            //}

            //coorPol[72] = AzimuthDist(center, 0, lag.numLags * lag.lagSize);
            //LinearRing pol = new LinearRing(coorPol);
            //Polygon poly = new Polygon(pol);
            border.Features.Add(Utils.DrawEllipse(new Kpoint(500000 + (size / 2), size / 2,0,0),majorR,minorR, az));

            
            return border;
        }
         
        public IRaster Surface( string outGridPath)
        {
            if (_lag == null)
                return null;

            _distancesA = new SortedList<int, double>();
           
            _semivarianceA = new SortedList<int, double>();
             _covarianceA = new SortedList<int, double>();
            double size = _lag.numLags * 2 * _lag.lagSize;
            Extent ext = new Extent(new double[4] {500000,0,500000+size,size });
            IRaster raster;
            AbstractInterpolator.CreateGridFromExtents(ext, _lag.lagSize, KnownCoordinateSystems.Projected.UtmWgs1984.WGS1984UTMZone10N, -1, outGridPath, out raster);

            int nr = raster.NumRows;
            int nc = raster.NumColumns;
            int i = 0;
            for (int row = 0; row < nr; row++)
            {
                for (int col = 0; col < nc; col++)
                {
                    if (col >= (_lag.numLags * 2) || row >= (_lag.numLags * 2))
                    { }
                    else
                    {
                        if (_spatConti[row, col].num > 0 && GetDistanceGrid(row, col) <= (_lag.numLags  * _lag.lagSize))
                        {

                            raster.Value[row, col] = _spatConti[row, col].Mean;
                           // distancesA.Add(i, spatDist[row, col].Mean);
                            _distancesA.Add(i,GetDistanceGrid(row,col));
                            _semivarianceA.Add(i, _spatConti[row, col].Mean);
                            _covarianceA.Add(i, _spatContiCov[row, col].Result());
                            i++;
                        }
                    }
                }
            }
            raster.Save();
            raster.Close();
            _calculated = true;
            return raster;
        }
         
        public Coordinate AzimuthDist(Coordinate ptoI, double azi, double dist)
        {
                return new Coordinate(ptoI.X + Math.Sin(azi) * dist, ptoI.Y + Math.Cos(azi) * dist);
        }
        */

        private void calculateListValues() {
            if (_lag == null) {
                return;
            }

            _distancesA = new SortedList<int, double>();
            _semivarianceA = new SortedList<int, double>();
            _covarianceA = new SortedList<int, double>();
            double size = _lag.numLags*2*_lag.lagSize;

            int nr = (_lag.numLags*2);
            int nc = (_lag.numLags*2);
            int i = 0;
            for (int row = 0; row < nr; row++) {
                for (int col = 0; col < nc; col++) {
                    if (col >= (_lag.numLags*2) ||
                        row >= (_lag.numLags*2)) {
                    } else {
                        if (_spatConti[row, col].num > 0 &&
                            getDistanceGrid(row, col) <= (_lag.numLags*_lag.lagSize)) {
                            _distancesA.Add(i, getDistanceGrid(row, col));
                            _semivarianceA.Add(i, _spatConti[row, col].Mean);
                            _covarianceA.Add(i, _spatContiCov[row, col].result());
                            i++;
                        }
                    }
                }
                _calculated = true;
            }
        }

        public double getDistanceGrid(int row, int col) {
            if (_lag == null) {
                return 0;
            }

            double initialcol = LagSize/2;
            double initialrow = LagSize/2;
            double len = NumberOfLags*LagSize;
            double x, y;

            x = (len) - initialcol - ((col)*LagSize);
            y = (len) - initialcol - ((row)*LagSize);

            return Math.Sqrt((x*x) + (y*y));
        }

        private double getAzimuth(int i, int j) {
            double dx = _gridPoints[j].X - _gridPoints[i].X;
            double dy = _gridPoints[j].Y - _gridPoints[i].Y;
            if (dx.Equals(0) &&
                dy > 0) {
                return 0;
            }
            if (dx.Equals(0) &&
                dy < 0) {
                return (Math.PI);
            }
            if (dx > 0 &&
                dy.Equals(0)) {
                return (Math.PI/2);
            }
            if (dx < 0 &&
                dy.Equals(0)) {
                return (3*Math.PI/2);
            }
            if (dx > 0 &&
                dy > 0) {
                return (Math.Atan(dx/dy));
            }
            if (dx > 0 &&
                dy < 0) {
                return (Math.PI + Math.Atan(dx/dy));
            }
            if (dx < 0 &&
                dy < 0) {
                return ((Math.PI) + Math.Atan(dx/dy));
            }
            if (dx < 0 &&
                dy > 0) {
                return ((2*Math.PI) + Math.Atan(dx/dy));
            }
            return 0;
        }

        /// <summary>
        /// Calculate the distastance between two points
        /// </summary>
        /// <param name="i">Position ith in the point array</param>
        /// <param name="j">Position jth in the point array</param>
        /// <returns>Return the distance</returns>
        private double mdistances(
            int i, int j) {
            return Math.Sqrt(((_gridPoints[i].X - _gridPoints[j].X)*(_gridPoints[i].X - _gridPoints[j].X)) +
                             ((_gridPoints[i].Y - _gridPoints[j].Y)*(_gridPoints[i].Y - _gridPoints[j].Y)));
        }


        /// <summary>
        /// Calculate the distastance between two points
        /// </summary>
        /// <param name="i">Position ith in the point array</param>
        /// <param name="j">Position jth in the point array</param>
        /// <returns>Return the distance</returns>
        private double[] mdistancesXy(
            int i, int j) {
            return new double[2] {(_gridPoints[j].X - _gridPoints[i].X), (_gridPoints[j].Y - _gridPoints[i].Y)};
        }

        /// <summary>
        /// Calculate Semivariances
        /// </summary>
        /// <param name="i">Position ith in the point array</param>
        /// <param name="j">Position jth in the point array</param>
        /// <returns>the semivariance</returns>
        private double msemiVarEmp(
            int i, int j) {
            return
                (0.5*((_gridPoints[i].Z - _gridPoints[j].Z)*(_gridPoints[i].Z - _gridPoints[j].Z))).GetValueOrDefault();
        }

        /// <summary>
        /// This method calculate the distances between all points
        /// </summary>
        private void calculateDistances() {
            double[] mdisv;
            double mdis;
            double msem;
            for (int i = 0; i < _npoints; i++) {
                for (int j = i; j < _npoints; j++) {
                    mdisv = mdistancesXy(i, j);
                    msem = msemiVarEmp(i, j);
                    mdis = mdistances(i, j);

                    if (!mdisv[0].Equals(0) &&
                        !mdisv[1].Equals(0)) {
                        Stat semi = new Stat(msem);
                        Covariance cov = new Covariance(_gridPoints[j].Z.GetValueOrDefault(),
                            _gridPoints[i].Z.GetValueOrDefault());
                        Stat dist = new Stat(mdis);

                        _distance += dist;
                        _semivariance += semi;
                    }
                }
            }
        }

        /// <summary>
        /// This method calculate the distances between all points
        /// </summary>
        private void calculateDistancesLag() {
            for (int i = 0; i < _npoints; i++) {
                for (int j = i; j < _npoints; j++) {
                    var mdisv = mdistancesXy(i, j);
                    var msem = msemiVarEmp(i, j);
                    var mdis = mdistances(i, j);

                    if (!mdisv[0].Equals(0) &&
                        !mdisv[1].Equals(0)) {
                        int[] cells = _lag.cells(i, j, mdisv[0], mdisv[1], getAzimuth(i, j));

                        var semi = new Stat(msem);
                        var cov = new Covariance(_gridPoints[j].Z.GetValueOrDefault(),
                            _gridPoints[i].Z.GetValueOrDefault());
                        var dist = new Stat(mdis);

                        var checkBorder0 = checkBorder(cells[0]);
                        var checkBorder1 = checkBorder(cells[1]);
                        var checkBorder2 = checkBorder(cells[2]);
                        var checkBorder3 = checkBorder(cells[3]);

                        int v = (NumberOfLags*2) - 1;
                        if (checkBorder2 && checkBorder0) {
                            _spatConti[cells[2], cells[0]] += semi;
                            _spatContiCov[cells[2], cells[0]] += cov;
                            _spatDist[cells[2], cells[0]] += dist;

                            _spatConti[v - cells[2], v - cells[0]] += semi;
                            _spatContiCov[v - cells[2], v - cells[0]] += cov;
                            _spatDist[v - cells[2], v - cells[0]] += dist;
                        }

                        if (checkBorder3 && checkBorder0) {
                            _spatConti[cells[3], cells[0]] += semi;
                            _spatContiCov[cells[3], cells[0]] += cov;
                            _spatDist[cells[3], cells[0]] += dist;

                            _spatConti[v - cells[3], v - cells[0]] += semi;
                            _spatContiCov[v - cells[3], v - cells[0]] += cov;
                            _spatDist[v - cells[3], v - cells[0]] += dist;
                        }
                        if (checkBorder2 && checkBorder1) {
                            _spatConti[cells[2], cells[1]] += semi;
                            _spatContiCov[cells[2], cells[1]] += cov;
                            _spatDist[cells[2], cells[1]] += dist;

                            _spatConti[v - cells[2], v - cells[1]] += semi;
                            _spatContiCov[v - cells[2], v - cells[1]] += cov;
                            _spatDist[v - cells[2], v - cells[1]] += dist;
                        }
                        if (checkBorder3 && checkBorder1) {
                            _spatConti[cells[3], cells[1]] += semi;
                            _spatContiCov[cells[3], cells[1]] += cov;
                            _spatDist[cells[3], cells[1]] += dist;

                            _spatConti[v - cells[3], v - cells[1]] += semi;
                            _spatContiCov[v - cells[3], v - cells[1]] += cov;
                            _spatDist[v - cells[3], v - cells[1]] += dist;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method calculate the distances between all points
        /// </summary>
        private void calculateDistancesLag(CancellationToken cancellationToken) {
            for (int i = 0; i < _npoints; i++) {
                for (int j = i; j < _npoints; j++) {
                    cancellationToken.ThrowIfCancellationRequested();

                    var mdisv = mdistancesXy(i, j);
                    var msem = msemiVarEmp(i, j);
                    var mdis = mdistances(i, j);

                    if (!mdisv[0].Equals(0) &&
                        !mdisv[1].Equals(0)) {
                        int[] cells = _lag.cells(i, j, mdisv[0], mdisv[1], getAzimuth(i, j));

                        var semi = new Stat(msem);
                        var cov = new Covariance(_gridPoints[j].Z.GetValueOrDefault(),
                            _gridPoints[i].Z.GetValueOrDefault());
                        var dist = new Stat(mdis);

                        var checkBorder0 = checkBorder(cells[0]);
                        var checkBorder1 = checkBorder(cells[1]);
                        var checkBorder2 = checkBorder(cells[2]);
                        var checkBorder3 = checkBorder(cells[3]);

                        int v = (NumberOfLags*2) - 1;
                        if (checkBorder2 && checkBorder0) {
                            _spatConti[cells[2], cells[0]] += semi;
                            _spatContiCov[cells[2], cells[0]] += cov;
                            _spatDist[cells[2], cells[0]] += dist;

                            _spatConti[v - cells[2], v - cells[0]] += semi;
                            _spatContiCov[v - cells[2], v - cells[0]] += cov;
                            _spatDist[v - cells[2], v - cells[0]] += dist;
                        }

                        if (checkBorder3 && checkBorder0) {
                            _spatConti[cells[3], cells[0]] += semi;
                            _spatContiCov[cells[3], cells[0]] += cov;
                            _spatDist[cells[3], cells[0]] += dist;

                            _spatConti[v - cells[3], v - cells[0]] += semi;
                            _spatContiCov[v - cells[3], v - cells[0]] += cov;
                            _spatDist[v - cells[3], v - cells[0]] += dist;
                        }
                        if (checkBorder2 && checkBorder1) {
                            _spatConti[cells[2], cells[1]] += semi;
                            _spatContiCov[cells[2], cells[1]] += cov;
                            _spatDist[cells[2], cells[1]] += dist;

                            _spatConti[v - cells[2], v - cells[1]] += semi;
                            _spatContiCov[v - cells[2], v - cells[1]] += cov;
                            _spatDist[v - cells[2], v - cells[1]] += dist;
                        }
                        if (checkBorder3 && checkBorder1) {
                            _spatConti[cells[3], cells[1]] += semi;
                            _spatContiCov[cells[3], cells[1]] += cov;
                            _spatDist[cells[3], cells[1]] += dist;

                            _spatConti[v - cells[3], v - cells[1]] += semi;
                            _spatContiCov[v - cells[3], v - cells[1]] += cov;
                            _spatDist[v - cells[3], v - cells[1]] += dist;
                        }
                    }
                }
            }
        }

        private bool checkBorder(int v) {
            if (v != int.MinValue) {
                if (v >= 0 &&
                    v <= ((_lag.numLags*2) - 1)) {
                    return true;
                }
                return false;
            }
            return false;
        }


        public double[] getDistancesValues() {
            return Distances;
        }

        public double[] getSemivariancesValues() {
            return Semivariances;
        }

        public Stat getSemivarianceStatistic() {
            return SemivarianceStatistic;
        }

        public Stat getDistanceStatistic() {
            return _distance;
        }

        public Lag getLag() {
            return LagValue;
        }
    }
}