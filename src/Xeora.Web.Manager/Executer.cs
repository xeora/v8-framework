﻿using System;
using System.Collections.Generic;
using System.Data;
using Xeora.Web.Manager.Execution;

namespace Xeora.Web.Manager
{
    public static class Executer
    {
        // This function is for external call out side of the project DO NOT DISABLE IT
        public static Basics.Execution.InvokeResult<T> InvokeBind<T>(Basics.Context.Request.HttpMethod httpMethod, Basics.Execution.Bind bind) =>
            Executer.InvokeBind<T>(httpMethod, bind, ExecuterTypes.Undefined);

        public static Basics.Execution.InvokeResult<T> InvokeBind<T>(Basics.Context.Request.HttpMethod httpMethod, Basics.Execution.Bind bind, ExecuterTypes executerType)
        {
            if (bind == null)
                throw new NoNullAllowedException("Requires bind!");
            // Check if BindInfo Parameters has been parsed!
            if (!bind.Ready)
                throw new Exception("Bind Parameters should be parsed first!");

            DateTime executionBegins = DateTime.Now;
            
            Basics.Execution.InvokeResult<T> rInvokeResult =
                new Basics.Execution.InvokeResult<T>(bind);

            object invokedObject = 
                ApplicationFactory.Prepare(bind.Executable).Invoke(
                    httpMethod,
                    bind.Classes,
                    bind.Procedure,
                    bind.Parameters.Values,
                    bind.InstanceExecution,
                    executerType
                );

            if (invokedObject is Exception exception)
                rInvokeResult.Exception = exception;
            else
                rInvokeResult.Result = (T)invokedObject;
            
            if (!Basics.Configurations.Xeora.Application.Main.PrintAnalysis) return rInvokeResult;
            
            double totalMs =
                DateTime.Now.Subtract(executionBegins).TotalMilliseconds;

            if (totalMs > Basics.Configurations.Xeora.Application.Main.AnalysisThreshold)
            {
                Basics.Logging.Current
                    .Warning(
                        "analysed - execution duration",
                        new Dictionary<string, object>
                        {
                            { "duration", totalMs },
                            { "bind", bind },
                        },
                        Basics.Helpers.Context.UniqueId
                    );
                return rInvokeResult;
            }
            
            Basics.Logging.Current
                .Information(
                    "analysed - execution duration",
                    new Dictionary<string, object>
                    {
                        { "duration", totalMs },
                        { "bind", bind },
                    },
                    Basics.Helpers.Context.UniqueId
                );

            return rInvokeResult;
        }

        public static object ExecuteStatement(IEnumerable<string> domainIdAccessTree, string statementBlockId, string statement, object[] parameters, bool cache)
        {
            Statement.Executable executableInfo =
                Statement.Factory.CreateExecutable(domainIdAccessTree, statementBlockId, statement, parameters != null && parameters.Length > 0, cache);

            if (executableInfo.Exception != null)
                return executableInfo.Exception;

            try
            {
                object invokedObject =
                    ApplicationFactory.Prepare(executableInfo.ExecutableName).Invoke(
                        Basics.Context.Request.HttpMethod.GET,
                        new [] { executableInfo.ClassName },
                        "Execute",
                        parameters,
                        false,
                        ExecuterTypes.Undefined
                    );

                if (invokedObject is Exception exception)
                    throw exception;

                return invokedObject;
            }
            catch (Exception e)
            {
                Basics.Logging.Current
                    .Error(
                        "Execution Exception...", 
                        new Dictionary<string, object>
                        {
                            { "message", e.Message },
                            { "trace", e.ToString() }
                        }
                    )
                    .Flush();

                return e;
            }
        }

        public static string GetPrimitiveValue(object methodResult)
        {
            if (methodResult != null &&
                (methodResult.GetType().IsPrimitive || methodResult is string))
                return (string)methodResult;

            return null;
        }
    }
}
