﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Xeora.Web.Application.Controls;
using Xeora.Web.Basics;
using Xeora.Web.Basics.Domain;
using Xeora.Web.Basics.Domain.Control.Definitions;
using Xeora.Web.Global;
using Single = Xeora.Web.Directives.Elements.Single;

namespace Xeora.Web.Directives
{
    public class Mother : IMother
    {
        private readonly Single _SingleDirective;

        public event ParsingHandler ParseRequested;
        public event InstanceHandler InstanceRequested;
        public event DeploymentAccessHandler DeploymentAccessRequested;
        public event ControlResolveHandler ControlResolveRequested;

        public Mother(
            Single singleDirective, 
            Basics.ControlResult.Message messageResult, 
            IReadOnlyCollection<string> updateBlockIdList
        )
        {
            this.PropertyLock = new object();
            
            this.Pool = new DirectivePool();
            
            this.MessageResult = messageResult;
            this.RequestedUpdateBlockIds = 
                new List<string>();
            if (updateBlockIdList != null && updateBlockIdList.Count > 0)
                this.RequestedUpdateBlockIds.AddRange(updateBlockIdList);
            
            this._SingleDirective = singleDirective;
            this._SingleDirective.Mother = this;
            
            this.AnalysisBulk = new ConcurrentDictionary<DirectiveTypes, Tuple<int, double>>();
        }

        public object PropertyLock { get; }

        public ConcurrentDictionary<DirectiveTypes, Tuple<int, double>> AnalysisBulk { get; }
        public DirectivePool Pool { get; }

        public Basics.ControlResult.Message MessageResult { get; }
        public List<string> RequestedUpdateBlockIds { get; }

        public void RequestParsing(string rawValue, DirectiveCollection childrenContainer, ArgumentCollection arguments) =>
            ParseRequested?.Invoke(rawValue, childrenContainer, arguments);

        public void RequestInstance(out IDomain instance)
        {
            instance = null; 
            InstanceRequested?.Invoke(out instance);
        }

        public void RequestDeploymentAccess(ref IDomain instance, out Deployment.Domain deployment)
        {
            deployment = null;
            DeploymentAccessRequested?.Invoke(ref instance, out deployment);
        }

        public void RequestControlResolve(string controlId, ref IDomain instance, out IBase control)
        {
            control = new Unknown();
            ControlResolveRequested?.Invoke(controlId, ref instance, out control);
        }

        public void Process()
        {
            if (this.RequestedUpdateBlockIds.Count > 0)
            {
                this._SingleDirective.Parse();
                
                IDirective result =
                    this._SingleDirective.Children.Find(this.RequestedUpdateBlockIds.Last());

                if (result == null)
                    return;

                this._SingleDirective.Children.Clear();
                this._SingleDirective.Children.Add(result);
            }

            this._SingleDirective.Render();

            this.CreateAnalysisBulkReport();
        }

        private void CreateAnalysisBulkReport()
        {
            if (!Configurations.Xeora.Application.Main.PrintAnalysis) return;

            IEnumerator<KeyValuePair<DirectiveTypes, Tuple<int, double>>> analysisBulkEnumerator =
                this.AnalysisBulk.GetEnumerator();
            try
            {
                while (analysisBulkEnumerator.MoveNext())
                {
                    var (key, value) = analysisBulkEnumerator.Current;

                    if (value.Item2 / value.Item1 > Configurations.Xeora.Application.Main.AnalysisThreshold)
                    {
                        Logging.Current
                            .Warning(
                                $"analysed(r) - {key}",
                                new Dictionary<string, object>
                                {
                                    { "count", value.Item1 },
                                    { "totalDuration", value.Item2 },
                                    { "approximateDuration", value.Item2 / value.Item1 }
                                },
                                Helpers.Context.UniqueId
                            );
                        continue;
                    }

                    Logging.Current
                        .Information(
                            $"analysed(r) - {key}",
                            new Dictionary<string, object>
                            {
                                { "count", value.Item1 },
                                { "totalDuration", value.Item2 },
                                { "approximateDuration", value.Item2 / value.Item1 }
                            },
                            Helpers.Context.UniqueId
                        );
                }
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                analysisBulkEnumerator.Dispose();
            }
        }

        public string Result => this._SingleDirective.Result;
        public bool HasInlineError => this._SingleDirective.HasInlineError;

        public static string CreateErrorOutput(Exception exception)
        {
            if (!Configurations.Xeora.Application.Main.Debugging) return string.Empty;
            
            string exceptionString = string.Empty;
            do
            {
                exceptionString =
                    string.Format(
                        @"<div align='left' style='border: solid 1px #660000; background-color: #ffffff'>
                                <div align='left' style='font-weight: bolder; color:#ffffff; background-color:#cc0000; padding: 4px;'>{0}</div>
                                <br/>
                                <div align='left' style='padding: 4px'>{1}{2}</div>
                              </div>",
                        exception.Message,
                        exception.Source,
                        !string.IsNullOrEmpty(exceptionString) ? string.Concat("<hr size='1px' />", exceptionString) : null
                    );

                exception = exception.InnerException;
            } while (exception != null);

            return exceptionString;
        }
    }
}