using System.ComponentModel;

namespace DefLab.common.kriging.semivariances {
    /// <summary>
    /// Модели вариограммы
    /// </summary>
    public enum KrigingModelType : long {
        [Description("Круговая")]
        circular = 0,
        [Description("Экспоненциальная")]
        exponential = 1 ,
        [Description("Гауссова")]
        gaussian = 2,
        [Description("Сферическая")]
        spherical = 3,
        //[Description("Линейная")]
        linear = 4
    }
}
