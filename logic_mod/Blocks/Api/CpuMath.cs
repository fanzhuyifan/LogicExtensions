﻿using Logic.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Blocks.Api
{
    public class CpuMath : ApiList
    {
        public override List<CpuApiFunc> Api => new List<CpuApiFunc>
        {
            new CpuApiFunc("abs", false, "absolute value",
                new Dictionary<string, CpuApiFunc.ArgInfo>{ { "value", new CpuApiFunc.ArgInfo("float", "value to apply abs") } },
                (c) => Abs
            ),
            new CpuApiFunc("ceil", false, "ceiling",
                new Dictionary<string, CpuApiFunc.ArgInfo>{ { "value", new CpuApiFunc.ArgInfo("float", "value to apply ceil") } },
                (c) => Ceiling
            ),
            new CpuApiFunc("floor", false, "floor",
                new Dictionary<string, CpuApiFunc.ArgInfo>{ { "value", new CpuApiFunc.ArgInfo("float", "value to apply floor") } },
                (c) => Floor
            ),
            new CpuApiFunc("round", false, "nearest integer",
                new Dictionary<string, CpuApiFunc.ArgInfo>{ { "value", new CpuApiFunc.ArgInfo("float", "value to apply round") } },
                (c) => Round
            ),
            new CpuApiFunc("sqrt", false, "square root",
                new Dictionary<string, CpuApiFunc.ArgInfo>{ { "value", new CpuApiFunc.ArgInfo("float", "value to apply square root") } },
                (c) => Sqrt
            ),
            new CpuApiFunc("pow", false, "power x^y",
                new Dictionary<string, CpuApiFunc.ArgInfo>{
                    { "x", new CpuApiFunc.ArgInfo("float", "value to apply") },
                    { "y", new CpuApiFunc.ArgInfo("float", "power") } },
                (c) => Pow
            ),
            new CpuApiFunc("sin", false, "sin trigonometry function",
                new Dictionary<string, CpuApiFunc.ArgInfo>{ { "value", new CpuApiFunc.ArgInfo("float", "value to apply sin") } },
                (c) => Sin
            ),
            new CpuApiFunc("cos", false, "cos trigonometry function",
                new Dictionary<string, CpuApiFunc.ArgInfo>{ { "value", new CpuApiFunc.ArgInfo("float", "value to apply cos") } },
                (c) => Cos
            ),
            new CpuApiFunc("tan", false, "tan trigonometry function",
                new Dictionary<string, CpuApiFunc.ArgInfo>{ { "value", new CpuApiFunc.ArgInfo("float", "value to apply tan") } },
                (c) => Tan
            ),
            new CpuApiFunc("asin", false, "asin trigonometry function",
                new Dictionary<string, CpuApiFunc.ArgInfo>{ { "value", new CpuApiFunc.ArgInfo("float", "value to apply asin") } },
                (c) => Asin
            ),
            new CpuApiFunc("acos", false, "acos trigonometry function",
                new Dictionary<string, CpuApiFunc.ArgInfo>{ { "value", new CpuApiFunc.ArgInfo("float", "value to apply acos") } },
                (c) => Acos
            ),
            new CpuApiFunc("atan", false, "atan trigonometry function",
                new Dictionary<string, CpuApiFunc.ArgInfo>{ { "value", new CpuApiFunc.ArgInfo("float", "value to apply atan") } },
                (c) => Atan
            ),
            new CpuApiFunc("log", false, "logarithm",
                new Dictionary<string, CpuApiFunc.ArgInfo>{
                    { "value", new CpuApiFunc.ArgInfo("float", "value to apply logarifm") },
                    { "newBase", new CpuApiFunc.ArgInfo("float", "logarithm base, defaults to e") }
                },
                (c) => Log
            ),
            new CpuApiFunc("exp", false, "exponential",
                new Dictionary<string, CpuApiFunc.ArgInfo>{ { "value", new CpuApiFunc.ArgInfo("float", "value to apply exp") } },
                (c) => Exp
            ),
            new CpuApiFunc("newton", false, "numerical solver",
                new Dictionary<string, CpuApiFunc.ArgInfo>{
                    { "func", new CpuApiFunc.ArgInfo("func", "The function whose zero is wanted. It must be a function of a single variable") },
                    { "x0", new CpuApiFunc.ArgInfo("float", "An initial estimate of the zero that should be somewhere near the actual zero.") },
                    { "fprime", new CpuApiFunc.ArgInfo("func", "(optional) The derivative of the function when available.") },
                    { "tol", new CpuApiFunc.ArgInfo("float", "(optional) The allowable error of the zero value.") },
                    { "maxiter", new CpuApiFunc.ArgInfo("int", "(optional) Maximum number of iterations.") },
                    { "fprime2", new CpuApiFunc.ArgInfo("func", "(optional) The second order derivative of the function when available.") },
                    { "x1", new CpuApiFunc.ArgInfo("float", "(optional) Estimate of the zero. Used if `fprime` is not provided.") },
                    { "rtol", new CpuApiFunc.ArgInfo("flot", "(optional) Tolerance (relative) for termination.") },
                    { "full_output", new CpuApiFunc.ArgInfo("bool", "(optional) Return just value (false) or object description (true).") }
                },
                (c) => Newton
            ),
        };

        public object Abs(VarCtx ctx, object[] x)
        {
            if (x.Length < 1 || !BlockUtils.TryGetFloat(x[0], out float v))
                throw new Exception("Invalid value");
            return (float)Math.Abs(v);
        }
        public object Ceiling(VarCtx ctx, object[] x)
        {
            if (x.Length < 1 || !BlockUtils.TryGetFloat(x[0], out float v))
                throw new Exception("Invalid value");
            // since int is not supported by the JS implementation, we will be
            // using long
            return (long)Math.Ceiling(v);
        }
        public object Floor(VarCtx ctx, object[] x)
        {
            if (x.Length < 1 || !BlockUtils.TryGetFloat(x[0], out float v))
                throw new Exception("Invalid value");
            return (long)Math.Floor(v);
        }
        public object Round(VarCtx ctx, object[] x)
        {
            if (x.Length < 1 || !BlockUtils.TryGetFloat(x[0], out float v))
                throw new Exception("Invalid value");
            return (long)Math.Round(v);
        }
        public object Pow(VarCtx ctx, object[] x)
        {
            if (x.Length < 2 || !BlockUtils.TryGetFloat(x[0], out float v) || !BlockUtils.TryGetFloat(x[1], out float y))
                throw new Exception("Invalid value");
            return (float)Math.Pow(v, y);
        }

        public object Sqrt(VarCtx ctx, object[] x)
        {
            if (x.Length < 1 || !BlockUtils.TryGetFloat(x[0], out float v))
                throw new Exception("Invalid value");
            return (float)Math.Sqrt(v);
        }
        public object Sin(VarCtx ctx, object[] x)
        {
            if (x.Length < 1 || !BlockUtils.TryGetFloat(x[0], out float v))
                throw new Exception("Invalid value");
            return (float)Math.Sin(v);
        }
        public object Cos(VarCtx ctx, object[] x)
        {
            if (x.Length < 1 || !BlockUtils.TryGetFloat(x[0], out float v))
                throw new Exception("Invalid value");
            return (float)Math.Cos(v);
        }
        public object Tan(VarCtx ctx, object[] x)
        {
            if (x.Length < 1 || !BlockUtils.TryGetFloat(x[0], out float v))
                throw new Exception("Invalid value");
            return (float)Math.Tan(v);
        }

        public object Asin(VarCtx ctx, object[] x)
        {
            if (x.Length < 1 || !BlockUtils.TryGetFloat(x[0], out float v))
                throw new Exception("Invalid value");
            return (float)Math.Asin(v);
        }
        public object Acos(VarCtx ctx, object[] x)
        {
            if (x.Length < 1 || !BlockUtils.TryGetFloat(x[0], out float v))
                throw new Exception("Invalid value");
            return (float)Math.Acos(v);
        }
        public object Atan(VarCtx ctx, object[] x)
        {
            if (x.Length < 1 || !BlockUtils.TryGetFloat(x[0], out float v))
                throw new Exception("Invalid value");
            return (float)Math.Atan(v);
        }
        public object Log(VarCtx ctx, object[] x)
        {
            float v, newBase;
            int l = x.Length;
            if (l < 1 || !BlockUtils.TryGetFloat(x[0], out v))
                throw new Exception("Invalid value");

            if (l == 1)
                return (float)Math.Log(v);
            else
            {
                if (!BlockUtils.TryGetFloat(x[1], out newBase))
                    throw new Exception("Invalid value");
                return (float)Math.Log(v, newBase);
            }
        }
        public object Exp(VarCtx ctx, object[] x)
        {
            if (x.Length < 1 || !BlockUtils.TryGetFloat(x[0], out float v))
                throw new Exception("Invalid value");
            return (float)Math.Exp(v);
        }

        public bool WithinTol(float x, float y, float atol, float rtol)
        {
            return Math.Abs(x - y) <= atol + rtol * Math.Abs(y);
        }

        // Find a zero of a real or complex function using the Newton-Raphson
        // (or secant or Halley’s) method.
        //
        // Math.newton(func, x0, fprime=null, tol=1.48e-08, maxiter=50, fprime2=null,
        // x1, rtol=0.0, full_output=false)
        //
        // Mimicks scipy's implementation of scipy.optimize.newton
        // 
        // Parameters
        // func: function
        //      The function whose zero is wanted. It must be a function of a
        //      single variable
        // x0: float
        //      An initial estimate of the zero that should be somewhere near
        //     the actual zero.
        // fprime : function, optional
        //      The derivative of the function when available and convenient. If it
        //      is null (default), then the secant method is used.
        // tol : float, optional
        //      The allowable error of the zero value.
        // maxiter : int, optional
        //      Maximum number of iterations.
        // fprime2 : function, optional
        //      The second order derivative of the function when available and
        //      convenient. If it is null (default), then the normal Newton-Raphson
        //      or the secant method is used. If it is not null, then Halley's method
        //      is used.
        // x1 : float, optional
        //      Another estimate of the zero that should be somewhere near the
        //      actual zero. Used if `fprime` is not provided.
        // rtol : float, optional
        //      Tolerance (relative) for termination.
        // full_output : bool, optional
        //      If `full_output` is false (default), the root is returned.
        //      If true, the dictionary {{"root": root}, {"converged": true/false},
        //      {"iter": numIter}} is returned.
        public object Newton(VarCtx ctx, object[] x)
        {
            int l = x.Length;

            // Arguments and their default values:
            object func;
            float x0;
            object fprime = null;
            float tol = 1.48e-08F;
            long maxiter = 50;
            object fprime2 = null;
            float x1 = 0;
            float rtol = 0.0F;
            bool full_output = false;

            bool x1Provided = false;
            object[] args = new object[1];

            if (l < 2)
                throw new Exception("Invalid value");

            // Conditionally initialize the arguments
            void Parse()
            {
                int curArgIndex = 0;

                func = x[curArgIndex++];

                if (!BlockUtils.TryGetFloat(x[curArgIndex++], out x0))
                    throw new Exception("Invalid value");

                if (curArgIndex >= l)
                    return;
                fprime = x[curArgIndex++];

                if (curArgIndex >= l)
                    return;
                if (!BlockUtils.TryGetFloat(x[curArgIndex++], out tol))
                    throw new Exception("Invalid value");

                if (curArgIndex >= l)
                    return;
                if (!BlockUtils.TryGetLong(x[curArgIndex++], out maxiter))
                    throw new Exception("Invalid value");

                if (curArgIndex >= l)
                    return;
                fprime2 = x[curArgIndex++];

                if (curArgIndex >= l)
                    return;
                x1Provided = BlockUtils.TryGetFloat(x[curArgIndex++], out x1);

                if (curArgIndex >= l)
                    return;
                if (!BlockUtils.TryGetFloat(x[curArgIndex++], out rtol))
                    throw new Exception("Invalid value");

                if (curArgIndex >= l)
                    return;
                full_output = BlockUtils.GetBool(x[curArgIndex++]);
            }

            Parse();

            if (tol <= 0)
                throw new Exception("tol too small (" + tol + " <= 0)");
            if (maxiter < 1)
                throw new Exception("maxiter must be greater than 0");
            float p0 = x0;
            long itr = 0;
            float p = 0;
            if (fprime is FuncCtx)
            {
                // Newton - Raphson method
                for (; itr < maxiter; ++itr)
                {
                    // first evaluate fval
                    args[0] = p0;
                    float fval = (float)Block.CallFunc(ctx, func, args);
                    // if fval is 0, a root has been found, then terminate
                    if (fval == 0)
                        return _newton_result_select(full_output, p0, itr, converged: true);
                    float fder = (float)Block.CallFunc(ctx, fprime, args);
                    // stop iterating if the derivative is zero
                    if (fder == 0)
                        return _newton_result_select(full_output, p0, itr + 1, converged: false);

                    // Newton step
                    float newton_step = fval / fder;
                    if (fprime2 is FuncCtx)
                    {
                        float fder2 = (float)Block.CallFunc(ctx, fprime2, args);
                        // Halley's method:
                        // newton_step /= (1.0 - 0.5 * newton_step * fder2 / fder)
                        // Only do it if denominator stays close enough to 1
                        // Rationale:  If 1-adj < 0, then Halley sends x in the
                        // opposite direction to Newton.  Doesn't happen if x is close
                        // enough to root.
                        float adj = newton_step * fder2 / fder / 2;
                        if (Math.Abs(adj) < 1)
                            newton_step /= 1.0F - adj;
                    }
                    p = p0 - newton_step;
                    if (WithinTol(p, p0, atol: tol, rtol: rtol))
                        return _newton_result_select(full_output, p, itr + 1, converged: true);
                    p0 = p;
                }
            }
            else
            {
                // secant method
                float p1, q0, q1;
                if (x1Provided)
                {
                    if (x1 == x0)
                        throw new Exception("x1 and x0 must be different");
                    p1 = x1;
                }
                else
                {
                    float eps = 1e-4F;
                    p1 = x0 * (1 + eps);
                    p1 += (p1 >= 0 ? eps : -eps);
                }
                args[0] = p0;
                q0 = (float)Block.CallFunc(ctx, func, args);
                args[0] = p1;
                q1 = (float)Block.CallFunc(ctx, func, args);
                if (Math.Abs(q1) < Math.Abs(q0))
                {
                    float temp = q1;
                    q1 = q0;
                    q0 = temp;

                    temp = p0;
                    p0 = p1;
                    p1 = temp;
                }
                for (; itr < maxiter; ++itr)
                {
                    if (q0 == q1)
                    {
                        p = (p1 + p0) / 2.0F;
                        if (p1 != p0)
                            return _newton_result_select(full_output, p, itr + 1, converged: false);
                        else
                            return _newton_result_select(full_output, p, itr + 1, converged: true);

                    }
                    else
                    {
                        // Secant Step
                        if (Math.Abs(q1) > Math.Abs(q0))
                            p = (-q0 / q1 * p1 + p0) / (1.0F - q0 / q1);
                        else
                            p = (-q1 / q0 * p0 + p1) / (1.0F - q1 / q0);
                    }
                    if (WithinTol(p, p1, atol: tol, rtol: rtol))
                        return _newton_result_select(full_output, p, itr + 1, converged: true);

                    p0 = p1;
                    q0 = q1;
                    p1 = p;
                    args[0] = p1;
                    q1 = (float)Block.CallFunc(ctx, func, args);
                }
            }
            return _newton_result_select(full_output, p, itr + 1, converged: false);
        }

        private object _newton_result_select(bool full_output, float p0,
            long itr, bool converged)
        {
            if (full_output)
            {
                return new Dictionary<string, object>()
                {
                    {"root", p0 },
                    {"iter", itr },
                    {"converged", converged }
                };
            }
            else
                return p0;
        }
    }
}
