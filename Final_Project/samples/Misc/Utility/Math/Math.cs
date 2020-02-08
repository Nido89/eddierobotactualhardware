//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: Math.cs $ $Revision: 13 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Microsoft.Dss.ServiceModel.Dssp;
using System.Security.Permissions;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using math = System.Math;
using Microsoft.Ccr.Core;
using W3C.Soap;

namespace Microsoft.Robotics.Services.Sample.Math
{
    /// <summary>
    /// MathService - Mathematical functions
    /// </summary>
    [DisplayName("(User) Math Functions")]
    [Description("Provides access to simple mathematical functions.")]
    [Contract(Contract.Identifier)]
    [DssServiceDescription("http://msdn.microsoft.com/library/dd126868.aspx")]
    public class MathService : DsspServiceBase
    {
        [ServicePort("math", AllowMultipleInstances = false)]
        MathOperations _mainPort = new MathOperations();

        MathConstants _constants = new MathConstants();
        System.Random _random = new System.Random();

        /// <summary>
        /// Constructor for service
        /// </summary>
        /// <param name="creationPort"></param>
        public MathService(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        /// <summary>
        /// Service Start
        /// </summary>
        protected override void Start()
        {
            base.Start();
        }

        /// <summary>
        /// ConstantsHandler - Return constants (Pi, E)
        /// </summary>
        /// <param name="constants"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> ConstantsHandler(Constants constants)
        {
            constants.ResponsePort.Post(_constants);
            yield break;
        }

        /// <summary>
        /// AcosHandler - Process Arc Cosine (math.Acos(D))
        /// </summary>
        /// <param name="acos"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> AcosHandler(Acos acos)
        {
            acos.ResponsePort.Post(new Response(math.Acos(acos.Body.D)));
            yield break;
        }

        /// <summary>
        /// AsinHandler - Process Arc Sine (math.Asin(D))
        /// </summary>
        /// <param name="asin"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> AsinHandler(Asin asin)
        {
            asin.ResponsePort.Post(new Response(math.Asin(asin.Body.D)));
            yield break;
        }

        /// <summary>
        /// AtanHandler - Process Arc Tangent (math.Atan(D))
        /// </summary>
        /// <param name="atan"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> AtanHandler(Atan atan)
        {
            atan.ResponsePort.Post(new Response(math.Atan(atan.Body.D)));
            yield break;
        }

        /// <summary>
        /// Atan2Handler - Process Arc Tangent (math.Atan2(Y, X))
        /// </summary>
        /// <param name="atan2"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> Atan2Handler(Atan2 atan2)
        {
            atan2.ResponsePort.Post(new Response(math.Atan2(atan2.Body.Y, atan2.Body.X)));
            yield break;
        }

        /// <summary>
        /// CosHandler - Process Cosine (math.Cos(Angle))
        /// </summary>
        /// <param name="cos"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> CosHandler(Cos cos)
        {
            cos.ResponsePort.Post(new Response(math.Cos(cos.Body.Angle)));
            yield break;
        }

        /// <summary>
        /// ExpHandler - Process Exponential (math.Exp(Exponent))
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> ExpHandler(Exp exp)
        {
            exp.ResponsePort.Post(new Response(math.Exp(exp.Body.Exponent)));
            yield break;
        }

        /// <summary>
        /// LogHandler - Process Logarithm (math.Log(A, Base))
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> LogHandler(Log log)
        {
#if URT_MINCLR
            if (log.Body.Base == 0 ||
                log.Body.Base == math.E)
            {
                log.ResponsePort.Post(new Response(math.Log(log.Body.A)));
            }
            else if (log.Body.Base == 10)
            {
                log.ResponsePort.Post(new Response(math.Log10(log.Body.A)));
            }
            else
            {
                log.ResponsePort.Post(
                    Fault.FromCodeSubcodeReason(
                        FaultCodes.Receiver,
                        DsspFaultCodes.ActionNotSupported,
                        "Only natural logarithms and logarithms in base 10 are supported."
                    )
                );
            }
#else
            double logbase;

            if (log.Body.Base == 0)
            {
                logbase = math.E;
            }
            else
            {
                logbase = log.Body.Base;
            }
            log.ResponsePort.Post(new Response(math.Log(log.Body.A, logbase)));
#endif
            yield break;
        }

        /// <summary>
        /// PowHandler - Process Raise to a Power (math.Pow(A, Exponent))
        /// </summary>
        /// <param name="pow"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> PowHandler(Pow pow)
        {
            pow.ResponsePort.Post(new Response(math.Pow(pow.Body.A, pow.Body.Exponent)));
            yield break;
        }

        /// <summary>
        /// RandomHandler - Return a random number (double)
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> RandomHandler(Random random)
        {
            random.ResponsePort.Post(new Response(_random.NextDouble()));
            yield break;
        }

        /// <summary>
        /// RoundHandler - Process Rounding (math.Round(Value, Digits))
        /// </summary>
        /// <param name="round"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> RoundHandler(Round round)
        {
            round.ResponsePort.Post(new Response(math.Round(round.Body.Value, round.Body.Digits)));
            yield break;
        }

        /// <summary>
        /// SinHandler - Process Sine (math.Sin(Angle))
        /// </summary>
        /// <param name="sin"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> SinHandler(Sin sin)
        {
            sin.ResponsePort.Post(new Response(math.Sin(sin.Body.Angle)));
            yield break;
        }

        /// <summary>
        /// SqrtHandler - Process Square Root (math.Sqrt(A))
        /// </summary>
        /// <param name="sqrt"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> SqrtHandler(Sqrt sqrt)
        {
            sqrt.ResponsePort.Post(new Response(math.Sqrt(sqrt.Body.A)));
            yield break;
        }

        /// <summary>
        /// TanHandler - Process Tangent (math.Tan(Angle))
        /// </summary>
        /// <param name="tan"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> TanHandler(Tan tan)
        {
            tan.ResponsePort.Post(new Response(math.Tan(tan.Body.Angle)));
            yield break;
        }

        /// <summary>
        /// TruncateHandler - Process Truncate (math.Truncate(A))
        /// </summary>
        /// <param name="truncate"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> TruncateHandler(Truncate truncate)
        {
            double value;

#if URT_MINCLR
            if (truncate.Body.A >= 0.0)
            {
                value = math.Floor(truncate.Body.A);
            }
            else
            {
                value = math.Ceiling(truncate.Body.A);
            }
#else
            value = math.Truncate(truncate.Body.A);
#endif

            truncate.ResponsePort.Post(new Response(value));
            yield break;
        }

        /// <summary>
        /// ToRadiansHandler - Convert to Radians from Degrees
        /// </summary>
        /// <param name="toRadians"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> ToRadiansHandler(ToRadians toRadians)
        {
            toRadians.ResponsePort.Post(new Response(
                toRadians.Body.Angle * math.PI / 180.0
            ));

            yield break;
        }

        /// <summary>
        /// ToDegreesHandler - Convert to Degrees from Radians
        /// </summary>
        /// <param name="toDegrees"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> ToDegreesHandler(ToDegrees toDegrees)
        {
            toDegrees.ResponsePort.Post(new Response(
                toDegrees.Body.Angle * 180.0 / math.PI
            ));
            yield break;
        }

    }
}
