namespace DefLab.common.kriging.spatial
{
    public interface ISpatialContinuityData
    {
        Lag getLag();
        Stat getDistanceStatistic();
        Stat getSemivarianceStatistic();
        double[] getDistancesValues();
        double[] getSemivariancesValues();
    }
}
