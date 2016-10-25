using System;
using DefLab.common.kriging.spatial;

namespace DefLab.common.kriging.semivariances {
    public class TheoreticalModel : ICloneable {
        private Parameter _msill;
        private Parameter _mrange;
        private Parameter _mnugget;
        private Parameter _distance;
        private Func<double> _regressionFunction;
        private Parameter[] _regressionParameters;
        private Parameter[] _observedParameters;
        private IKrigingModel _krigingModel;
        private ISpatialContinuityData _spatial;
        private double[,] _z;
        private bool _sill;
        private bool _range = true;
        private bool _nugget;

        public event EventHandler ChangedKriginParameter;

        public ISpatialContinuityData Spatial
        {
            get { return _spatial; }
            set
            {
                _spatial = value;
                _z = new double[2, _spatial.getDistancesValues().Length];
                for (int i = 0; i < _spatial.getDistancesValues().Length; i++) {
                    _z[0, i] = _spatial.getDistancesValues()[i];
                    _z[1, i] = _spatial.getSemivariancesValues()[i];
                }
            }
        }

        public void fitValues(bool sill, bool range, bool nugget) {
            _sill = sill;
            _range = range;
            _nugget = nugget;
        }

        public IKrigingModel KrigingModel
        {
            get { return _krigingModel; }
            set
            {
                _krigingModel = value;
                onChangedKriginParameter(EventArgs.Empty);
            }
        }

        protected virtual void onChangedKriginParameter(EventArgs e) {
            var h = ChangedKriginParameter;
            if (h != null) {
                h(this, e);
            }
        }

        public TheoreticalModel(ISpatialContinuityData spatial) {
            _spatial = spatial;
        }

        public IKrigingModel getFunctionDefault() {
            getFunction(KrigingModelType.circular, _spatial.getSemivarianceStatistic().Mean,
                _spatial.getLag().lagSize*_spatial.getLag().numLags/2, 0);
            return getFunction(_krigingModel.Range);
        }

        public IKrigingModel getFunctionDefault(KrigingModelType model) {
            return getFunction(model, _spatial.getSemivarianceStatistic().Mean,
                _spatial.getLag().lagSize*_spatial.getLag().numLags/2, 0);
        }

        public IKrigingModel setFunction(KrigingModelType model, double sill, double range, double nugget) {
            _krigingModel = selectModel(model);
            _krigingModel.Nugget = nugget;
            _krigingModel.Sill = sill;
            _krigingModel.Range = range;
            onChangedKriginParameter(EventArgs.Empty);
            return _krigingModel;
        }

        public static IKrigingModel selectModel(KrigingModelType value) {
            IKrigingModel model;
            switch (value) {
                case KrigingModelType.circular:
                    model = new ModelCircular();
                    break;
                case KrigingModelType.exponential:
                    model = new ModelExponential();
                    break;
                case KrigingModelType.gaussian:
                    model = new ModelGaussian();
                    break;
                case KrigingModelType.linear:
                    model = new ModelLineal();
                    break;
                case KrigingModelType.spherical:
                    model = new ModelSpherical();
                    break;
                default:
                    model = new ModelExponential();
                    break;
            }
            return model;
        }

        private void defineParametersCalculation() {
            if (_sill &&
                _range &&
                _nugget) {
                _regressionParameters = new[] {_mrange, _msill, _mnugget};
            } else if (_sill && _range) {
                _regressionParameters = new[] {_mrange, _msill};
            } else if (_sill && _nugget) {
                _regressionParameters = new[] {_msill, _mnugget};
            } else if (_sill) {
                _regressionParameters = new[] {_msill};
            } else if (_range && _nugget) {
                _regressionParameters = new[] {_mrange, _mnugget};
            } else if (_range) {
                _regressionParameters = new[] {_mrange};
            } else if (_nugget) {
                _regressionParameters = new[] {_mnugget};
            } else {
                _regressionParameters = new Parameter[] {};
            }
        }

        private void defineParametersCalculationValues(double sillv, double rangev, double nuggetv) {
            if (nuggetv < 0) {
                nuggetv = 0;
            }


            if (_sill &&
                _range &&
                _nugget) {
                // regressionParameters = new Parameter[] { mrange, msill, mnugget };
                _krigingModel.Range = _regressionParameters[0].Value;
                _krigingModel.Sill = _regressionParameters[1].Value;
                if (_regressionParameters[2].Value < 0) {
                    _krigingModel.Nugget = 0;
                } else {
                    _krigingModel.Nugget = _regressionParameters[2].Value;
                }
            } else if (_sill && _range) {
                //   regressionParameters = new Parameter[] { mrange, msill };
                _krigingModel.Range = _regressionParameters[0].Value;
                _krigingModel.Sill = _regressionParameters[1].Value;
                _krigingModel.Nugget = nuggetv;
            } else if (_sill && _nugget) {
                //  regressionParameters = new Parameter[] { msill, mnugget };
                _krigingModel.Range = rangev;
                _krigingModel.Sill = _regressionParameters[0].Value;
                if (_regressionParameters[1].Value < 0) {
                    _krigingModel.Nugget = 0;
                } else {
                    _krigingModel.Nugget = _regressionParameters[1].Value;
                }
            } else if (_sill) {
                //  regressionParameters = new Parameter[] { msill };
                _krigingModel.Range = rangev;
                _krigingModel.Sill = _regressionParameters[0].Value;
                _krigingModel.Nugget = nuggetv;
            } else if (_range && _nugget) {
                //   regressionParameters = new Parameter[] { mrange, mnugget };
                _krigingModel.Range = _regressionParameters[0].Value;
                _krigingModel.Sill = sillv;
                if (_regressionParameters[1].Value < 0) {
                    _krigingModel.Nugget = 0;
                } else {
                    _krigingModel.Nugget = _regressionParameters[1].Value;
                }
            } else if (_range) {
                //   regressionParameters = new Parameter[] { mrange };
                _krigingModel.Range = _regressionParameters[0].Value;
                _krigingModel.Sill = sillv;
                _krigingModel.Nugget = nuggetv;
            } else if (_nugget) {
                // regressionParameters = new Parameter[] { mnugget };
                _krigingModel.Range = rangev;
                _krigingModel.Sill = sillv;
                if (_regressionParameters[0].Value < 0) {
                    _krigingModel.Nugget = 0;
                } else {
                    _krigingModel.Nugget = _regressionParameters[0].Value;
                }
            } else {
                //   regressionParameters = new Parameter[] { };
                _krigingModel.Range = rangev;
                _krigingModel.Sill = sillv;
                _krigingModel.Nugget = nuggetv;
            }
        }

        public IKrigingModel getFunction(double range, double nugget) {
            fitValues(false, true, true);
            return getFunction(_krigingModel.ModelType, _krigingModel.Sill, range, nugget);
        }

        public IKrigingModel getFunction(double range) {
            fitValues(true, false, false);
            return getFunction(_krigingModel.ModelType, _spatial.getSemivarianceStatistic().Mean, range, 0);
        }

        public IKrigingModel getFunction(KrigingModelType model, double sill, double range, double nugget) {
            _msill = new Parameter(sill);
            _mrange = new Parameter(range);
            _mnugget = new Parameter(nugget);
            _distance = new Parameter();


            // Parameter[] regressionParameters=null;
            //  Parameter[] observedParameters = null; ;
            _observedParameters = new[] {_distance};
            // regressionParameters = new Parameter[] { mrange, msill,mnugget };
            defineParametersCalculation();
            switch (model) {
                case KrigingModelType.circular:
                    _krigingModel = new ModelCircular();
                    _regressionFunction = () =>
                        _distance > 0 && _distance <= Math.Abs(_mrange)
                            ? _mnugget +
                              (((2*Math.Abs(_msill))/Math.PI)*
                               (((_distance/Math.Abs(_mrange))*
                                 Math.Sqrt(1 - ((_distance*_distance)/(Math.Abs(_mrange)*Math.Abs(_mrange))))) +
                                Math.Asin(_distance/Math.Abs(_mrange))))
                            : _distance > Math.Abs(_mrange) ? _mnugget + Math.Abs(_msill) : 0
                        ;
                    break;
                case KrigingModelType.exponential:
                    _krigingModel = new ModelExponential();
                    _regressionFunction = () => _distance > 0 ?
                        Math.Abs(_mnugget) + (Math.Abs(_msill)*(1 - Math.Exp(-3*Math.Abs(_distance)/Math.Abs(_mrange))))
                        : 0;
                    break;
                case KrigingModelType.gaussian:
                    _krigingModel = new ModelGaussian();
                    _regressionFunction = () => _distance > 0 ?
                        Math.Abs(_mnugget) +
                        (Math.Abs(_msill)*
                         (1 -
                          Math.Exp(-3*(Math.Abs(_distance)/Math.Abs(_mrange))*(Math.Abs(_distance)/Math.Abs(_mrange)))))
                        : 0;
                    break;
                case KrigingModelType.spherical:
                    _krigingModel = new ModelSpherical();
                    _regressionFunction = () =>
                        _distance > 0 && _distance <= Math.Abs(_mrange) ?
                            Math.Abs(_mnugget) + (Math.Abs(_msill)*((1.5*(_distance/Math.Abs(_mrange))) -
                                                                    (0.5*(_distance/Math.Abs(_mrange))*
                                                                     (_distance/Math.Abs(_mrange))*
                                                                     (_distance/Math.Abs(_mrange))))) :
                            _distance > Math.Abs(_mrange) ?
                                Math.Abs(_mnugget) + Math.Abs(_msill) :
                                0;

                    break;
                    case KrigingModelType.linear:
                    _krigingModel = new ModelLineal();
                    _regressionFunction = () =>
                      _distance < _mrange ?  _mnugget + ((_msill / _mrange) * Math.Abs(_distance)): _mnugget + _msill;

                    break;
            }

            if (_regressionParameters.Length == 0) {
                _krigingModel.Range = range;
                _krigingModel.Sill = sill;
                if (nugget < 0) {
                    _krigingModel.Nugget = 0;
                } else {
                    _krigingModel.Nugget = nugget;
                }
                onChangedKriginParameter(EventArgs.Empty);
                return _krigingModel;
            }

            if (_regressionParameters != null) {
                try {
                    var levenbergMarquardt = new LevenbergMarquardt(_regressionFunction, _regressionParameters,
                        _observedParameters, _z);

                    for (int i = 0; i < 50; i++) {
                        levenbergMarquardt.Iterate();
                    }
                    defineParametersCalculationValues(sill, range, nugget);
                } catch {
                    _krigingModel.Range = range;
                    _krigingModel.Sill = sill;
                    if (nugget < 0) {
                        _krigingModel.Nugget = 0;
                    } else {
                        _krigingModel.Nugget = nugget;
                    }
                    onChangedKriginParameter(EventArgs.Empty);
                    return _krigingModel;
                }
            }

            onChangedKriginParameter(EventArgs.Empty);
            return _krigingModel;
        }

        /// <summary>
        /// Возвращает вариограмму
        /// </summary>
        /// <param name="modelConfig"></param>
        /// <returns></returns>
        public static Func<double, double> getModelDelegate(KrigingModelConfig modelConfig)
        {
            var theoretical = new TheoreticalModel(null);
            var model = theoretical.getFunction(modelConfig);
            return model.getValue;
        }

        public IKrigingModel getFunction(KrigingModelConfig config) {
            return getFunction(config.ModelType, config.Sill, config.Range, config.Nugget);
        }


        public object Clone() {
            return new TheoreticalModel(_spatial) {KrigingModel = _krigingModel};
        }
    }
}