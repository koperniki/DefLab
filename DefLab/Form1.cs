using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DefLab.common.kriging;
using DefLab.common.kriging.semivariances;
using DefLab.common.math;
using DefLab.common.util;
using DefLab.models;
using DefLab.renderer;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg.ConfigurationSchema;
using ThreeCs.Math;

namespace DefLab
{
    public partial class Form1 : Form
    {
        private static ISessionFactory CreateSessionFactory()
        {
            return Fluently.Configure()
              .Database(
                SQLiteConfiguration.Standard
                  .UsingFile("DefLab.sqlite")
              ).Mappings(m => m.FluentMappings.AddFromAssemblyOf<Form1>())
              .BuildSessionFactory();
        }

        public Form1()
        {
            InitializeComponent();
           
            // testBd();
            //testKriging();
        }

        public void testGl()
        {
            var renderer = new KrigingRenderer();
            renderer.dataList = Mapper.gridFieldToVector3List(testKriging());
            /*new List<List<Vector3>>()
            {
                new List<Vector3>(5)
                {
                    new Vector3(0,0,0),
                    new Vector3(1,0,0),
                    new Vector3(1,1,0),
                    new Vector3(0,1,0),
                    new Vector3(0,0,0),
                }
            };*/

            // Mapper.gridFieldToVector3List( testKriging());
            rendererCtrl1.RunSample(renderer);
        }

        public GridField testKriging()
        {
         var config = new KrigingModelConfig();
            config.ModelType = KrigingModelType.linear;
            config.Nugget = 10;
            config.Range = 10;
            config.Sill = 10;
            var gridInfo = new GridInfo();
            gridInfo.Extent = new Extent(0,10, 0, 10);
            gridInfo.Step = 0.1;

            var krigingKonfig = new KrigingConfig();
            krigingKonfig.MaxPointsInSector = 10;
            krigingKonfig.SearchRadius = 10;
            krigingKonfig.SectorsCount = 1;

            var semivarianse = TheoreticalModel.getModelDelegate(config);

            var points = new[] {new GridPoint(1,1,5), new GridPoint(8,8,5) , new GridPoint(1, 8, 5), new GridPoint(8, 1, 5), new GridPoint(5, 5, 0) };

            var grid = Kriging.getGrid(gridInfo, false, krigingKonfig, semivarianse, points, null, null);
            return grid;
        }

        public void testBd()
        {

            var sessionFactory = CreateSessionFactory();
            using (var session = sessionFactory.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var test = new Test();
                    test.Data = "ololo";
                    session.SaveOrUpdate(test);
                    transaction.Commit();
                }
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            testGl();
        }
    }
}
