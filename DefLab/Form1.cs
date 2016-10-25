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
using DefLab.models;
using DefLab.renderer;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg.ConfigurationSchema;

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
            var renderer = new TestRenderer();
          rendererCtrl1.RunSample(renderer);
        }

        public void testKriging()
        {
         var config = new KrigingModelConfig();
            config.ModelType = KrigingModelType.linear;
            config.Nugget = 10;
            config.Range = 10;
            config.Sill = 10;
            var gridInfo = new GridInfo();
            gridInfo.Extent = new Extent(0,10, 0, 10);
            gridInfo.Step = 0.5;

            var krigingKonfig = new KrigingConfig();
            krigingKonfig.MaxPointsInSector = 20;
            krigingKonfig.SearchRadius = 10;
            krigingKonfig.SectorsCount = 1;

            var semivarianse = TheoreticalModel.getModelDelegate(config);

            var points = new[] {new GridPoint(1,1,5), new GridPoint(8,8,-4)  };

            var grid = Kriging.getGrid(gridInfo, false, krigingKonfig, semivarianse, points, null, null);
            ;
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
