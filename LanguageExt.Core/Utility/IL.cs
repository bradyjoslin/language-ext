﻿using LanguageExt.ClassInstances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;
using static LanguageExt.Prelude;
using static LanguageExt.Reflect;

namespace LanguageExt
{
    public static class IL
    {
        /// <summary>
        /// Emits the IL to instantiate a type of R with a single argument to 
        /// the constructor
        /// </summary>
        public static Func<R> Ctor<R>()
        {
            var ctorInfo = GetConstructor<R>()
                               .IfNone(() => throw new ArgumentException($"Constructor not found for type {typeof(R).FullName}"));

            var ctorParams = ctorInfo.GetParameters();

            var dynamic = new DynamicMethod("CreateInstance",
                                            ctorInfo.DeclaringType,
                                            new Type[0],
                                            true);

            var il = dynamic.GetILGenerator();
            il.Emit(OpCodes.Newobj, ctorInfo);
            il.Emit(OpCodes.Ret);

            return (Func<R>)dynamic.CreateDelegate(typeof(Func<R>));
        }

        /// <summary>
        /// Emits the IL to instantiate a type of R with a single argument to 
        /// the constructor
        /// </summary>
        public static Func<A, R> Ctor<A, R>()
        {
            var ctorInfo = GetConstructor<R, A>()
                               .IfNone(() => throw new ArgumentException($"Constructor not found for type {typeof(R).FullName}"));

            var ctorParams = ctorInfo.GetParameters();

            var dynamic = new DynamicMethod("CreateInstance",
                                            ctorInfo.DeclaringType,
                                            ctorParams.Select(p => p.ParameterType).ToArray(),
                                            true);

            var il = dynamic.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Newobj, ctorInfo);
            il.Emit(OpCodes.Ret);

            return (Func<A, R>)dynamic.CreateDelegate(typeof(Func<A, R>));
        }

        /// <summary>
        /// Emits the IL to instantiate a type of R with two arguments to 
        /// the constructor
        /// </summary>
        public static Func<A, B, R> Ctor<A, B, R>()
        {
            var ctorInfo = GetConstructor<R, A, B>()
                               .IfNone(() => throw new ArgumentException($"Constructor not found for type {typeof(R).FullName}"));

            var ctorParams = ctorInfo.GetParameters();

            var dynamic = new DynamicMethod("CreateInstance",
                                            ctorInfo.DeclaringType,
                                            ctorParams.Select(p => p.ParameterType).ToArray(),
                                            true);

            var il = dynamic.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Newobj, ctorInfo);
            il.Emit(OpCodes.Ret);

            return (Func<A, B, R>)dynamic.CreateDelegate(typeof(Func<A, B, R>));
        }

        /// <summary>
        /// Emits the IL to instantiate a type of R with three arguments to 
        /// the constructor
        /// </summary>
        public static Func<A, B, C, R> Ctor<A, B, C, R>()
        {
            var ctorInfo = GetConstructor<R, A, B, C>()
                               .IfNone(() => throw new ArgumentException($"Constructor not found for type {typeof(R).FullName}"));

            var ctorParams = ctorInfo.GetParameters();

            var dynamic = new DynamicMethod("CreateInstance",
                                            ctorInfo.DeclaringType,
                                            ctorParams.Select(p => p.ParameterType).ToArray(),
                                            true);

            var il = dynamic.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Newobj, ctorInfo);
            il.Emit(OpCodes.Ret);

            return (Func<A, B, C, R>)dynamic.CreateDelegate(typeof(Func<A, B, C, R>));
        }

        /// <summary>
        /// Emits the IL to instantiate a type of R with four arguments to 
        /// the constructor
        /// </summary>
        public static Func<A, B, C, D, R> Ctor<A, B, C, D, R>()
        {
            var ctorInfo = GetConstructor<R, A, B, C, D>()
                               .IfNone(() => throw new ArgumentException($"Constructor not found for type {typeof(R).FullName}"));

            if (ctorInfo == null) throw new ArgumentException($"Constructor not found for type {typeof(R).FullName}");

            var ctorParams = ctorInfo.GetParameters();

            var dynamic = new DynamicMethod("CreateInstance",
                                            ctorInfo.DeclaringType,
                                            ctorParams.Select(p => p.ParameterType).ToArray(),
                                            true);

            var il = dynamic.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldarg_3);
            il.Emit(OpCodes.Newobj, ctorInfo);
            il.Emit(OpCodes.Ret);

            return (Func<A, B, C, D, R>)dynamic.CreateDelegate(typeof(Func<A, B, C, D, R>));
        }

        /// <summary>
        /// Emits the IL to invoke a static method
        /// </summary>
        public static Option<Func<object, R>> Func1<TYPE, R>(Type arg1, Func<MethodInfo, bool> methodPred = null)
        {
            methodPred = methodPred ?? (_ => true);

            var methodInfo = typeof(TYPE)
                .GetTypeInfo()
                .GetAllMethods()
                .Where(x =>
                {
                    if (!x.IsStatic) return false;
                    var ps = x.GetParameters();
                    if (ps.Length != 1) return false;
                    if (ps[0].ParameterType != arg1) return false;
                    return methodPred(x);
                })
                .FirstOrDefault();

            if (methodInfo == null) return None;

            var methodParams = methodInfo.GetParameters();

            var dynamic = new DynamicMethod("CreateInstance",
                                            typeof(R),
                                            methodParams.Select(p => typeof(object)).ToArray(),
                                            true);

            var il = dynamic.GetILGenerator();
            il.DeclareLocal(typeof(R));
            il.Emit(OpCodes.Ldarg_0);
            if (arg1.GetTypeInfo().IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, arg1);
            }
            else
            {
                il.Emit(OpCodes.Castclass, arg1);
            }
            il.Emit(OpCodes.Call, methodInfo);
            il.Emit(OpCodes.Ret);

            return (Func<object, R>)dynamic.CreateDelegate(typeof(Func<object, R>));
        }

        /// <summary>
        /// Emits the IL to invoke a static method with one argument
        /// </summary>
        public static Option<Func<A, R>> Func1<TYPE, A, R>(Func<MethodInfo, bool> methodPred = null)
        {
            methodPred = methodPred ?? (_ => true);

            var methodInfo = typeof(TYPE)
                .GetTypeInfo()
                .GetAllMethods()
                .Where(x =>
                {
                    if (!x.IsStatic) return false;
                    var ps = x.GetParameters();
                    if (ps.Length != 1) return false;
                    if (ps[0].ParameterType != typeof(A)) return false;
                    return methodPred(x);
                })
                .FirstOrDefault();

            if (methodInfo == null) return None;

            var methodParams = methodInfo.GetParameters();

            var dynamic = new DynamicMethod("CreateInstance",
                                            typeof(R),
                                            methodParams.Select(p => p.ParameterType).ToArray(),
                                            true);

            var il = dynamic.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, methodInfo);
            il.Emit(OpCodes.Ret);

            return (Func<A, R>)dynamic.CreateDelegate(typeof(Func<A, R>));
        }

        /// <summary>
        /// Emits the IL to invoke a static method with two arguments
        /// </summary>
        public static Option<Func<A, B, R>> Func2<TYPE, A, B, R>(Func<MethodInfo, bool> methodPred = null)
        {
            methodPred = methodPred ?? (_ => true);

            var methodInfo = typeof(TYPE)
                .GetTypeInfo()
                .GetAllMethods()
                .Where(x =>
                {
                    if (!x.IsStatic) return false;
                    var ps = x.GetParameters();
                    if (ps.Length != 2) return false;
                    if (ps[0].ParameterType != typeof(A)) return false;
                    if (ps[1].ParameterType != typeof(B)) return false;
                    return methodPred(x);
                })
                .FirstOrDefault();

            if (methodInfo == null) return None;

            var methodParams = methodInfo.GetParameters();

            var dynamic = new DynamicMethod("CreateInstance",
                                            typeof(R),
                                            methodParams.Select(p => p.ParameterType).ToArray(),
                                            true);

            var il = dynamic.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, methodInfo);
            il.Emit(OpCodes.Ret);

            return (Func<A, B, R>)dynamic.CreateDelegate(typeof(Func<A, B, R>));
        }

        /// <summary>
        /// Emits the IL to invoke a static method with three arguments
        /// </summary>
        public static Option<Func<A, B, C, R>> Func3<TYPE, A, B, C, R>(Func<MethodInfo, bool> methodPred = null)
        {
            methodPred = methodPred ?? (_ => true);

            var methodInfo = typeof(TYPE)
                .GetTypeInfo()
                .GetAllMethods()
                .Where(x =>
                {
                    if (!x.IsStatic) return false;
                    var ps = x.GetParameters();
                    if (ps.Length != 3) return false;
                    if (ps[0].ParameterType != typeof(A)) return false;
                    if (ps[1].ParameterType != typeof(B)) return false;
                    if (ps[2].ParameterType != typeof(C)) return false;
                    return methodPred(x);
                })
                .FirstOrDefault();

            if (methodInfo == null) return None;

            var methodParams = methodInfo.GetParameters();

            var dynamic = new DynamicMethod("CreateInstance",
                                            typeof(R),
                                            methodParams.Select(p => p.ParameterType).ToArray(),
                                            true);

            var il = dynamic.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Call, methodInfo);
            il.Emit(OpCodes.Ret);

            return (Func<A, B, C, R>)dynamic.CreateDelegate(typeof(Func<A, B, C, R>));
        }

        /// <summary>
        /// Emits the IL to invoke a static method with four arguments
        /// </summary>
        public static Option<Func<A, B, C, D, R>> Func4<TYPE, A, B, C, D, R>(Func<MethodInfo, bool> methodPred = null)
        {
            methodPred = methodPred ?? (_ => true);

            var methodInfo = typeof(TYPE)
                .GetTypeInfo()
                .GetAllMethods()
                .Where(x =>
                {
                    if (!x.IsStatic) return false;
                    var ps = x.GetParameters();
                    if (ps.Length != 4) return false;
                    if (ps[0].ParameterType != typeof(A)) return false;
                    if (ps[1].ParameterType != typeof(B)) return false;
                    if (ps[2].ParameterType != typeof(C)) return false;
                    if (ps[3].ParameterType != typeof(D)) return false;
                    return methodPred(x);
                })
                .FirstOrDefault();

            if (methodInfo == null) return None;

            var methodParams = methodInfo.GetParameters();

            var dynamic = new DynamicMethod("CreateInstance",
                                            typeof(R),
                                            methodParams.Select(p => p.ParameterType).ToArray(),
                                            true);

            var il = dynamic.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldarg_3);
            il.Emit(OpCodes.Call, methodInfo);
            il.Emit(OpCodes.Ret);

            return (Func<A, B, C, D, R >)dynamic.CreateDelegate(typeof(Func<A, B, C, D, R>));
        }

        /// <summary>
        /// Builds a function to provide a hash-code for a record type.  the hash-code is built from
        /// the hash-codes of all the *fields* that make up the type.  
        /// </summary>
        /// <remarks>You should cache the result of this method to reduce the work of building the IL 
        /// each time.  Better still use the `RecordType<A>` type to provide a cached version of these
        /// results.
        /// </remarks>
        public static Func<A, int> GetHashCode<A>()
        {
            var dynamic = new DynamicMethod("GetHashCode", typeof(int), new[] { typeof(A) }, true);
            var fields = GetPublicInstanceFields<A>(typeof(OptOutOfHashCodeAttribute));
            var il = dynamic.GetILGenerator();
            bool isValueType = typeof(A).GetTypeInfo().IsValueType;

            // Load constant -2128831035
            il.Emit(OpCodes.Ldc_I4, -2128831035);

            foreach (var field in fields)
            {
                // Load constant 16777619
                il.Emit(OpCodes.Ldc_I4, 16777619);

                if (isValueType)
                {
                    // Load A
                    il.Emit(OpCodes.Ldarga_S, 0);
                }
                else
                {
                    // Load A
                    il.Emit(OpCodes.Ldarg_0);
                }

                if (field.FieldType.GetTypeInfo().IsValueType)
                {
                    // Load field
                    il.Emit(OpCodes.Ldflda, field);

                    // Call field GetHashCode
                    var method = field.FieldType.GetTypeInfo()
                                                .GetAllMethods()
                                                .Where(m => m.Name == "GetHashCode")
                                                .Where(m => default(EqArray<EqDefault<Type>, Type>).Equals(
                                                               m.GetParameters().Map(p => p.ParameterType).ToArray(),
                                                               new Type[0]))
                                                .First();
                    il.Emit(OpCodes.Call, method);
                }
                else
                {
                    var notNull = il.DefineLabel();
                    var useZero = il.DefineLabel();

                    // Load field
                    il.Emit(isValueType ? OpCodes.Ldflda : OpCodes.Ldfld, field);

                    // Duplicate top item on stack (2 of the same value on stack)
                    il.Emit(OpCodes.Dup);

                    // Test if null
                    il.Emit(OpCodes.Brtrue_S, notNull);

                    // Is null so load 0
                    il.Emit(OpCodes.Pop);
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Br_S, useZero);

                    il.MarkLabel(notNull);

                    // Not null so call GetHashCode
                    var method = field.FieldType.GetTypeInfo()
                                                .GetAllMethods()
                                                .Where(m => m.Name == "GetHashCode")
                                                .Where(m => default(EqArray<EqDefault<Type>, Type>).Equals(
                                                               m.GetParameters().Map(p => p.ParameterType).ToArray(),
                                                               new Type[0]))
                                                .First();
                    il.Emit(OpCodes.Callvirt, method);

                    il.MarkLabel(useZero);

                }
                // hashCode = hashCode ^ 16777619 + field.GetHashCode()
                il.Emit(OpCodes.Add);
                il.Emit(OpCodes.Xor);
            }
            il.Emit(OpCodes.Ret);

            return (Func<A, int>)dynamic.CreateDelegate(typeof(Func<A, int>));
        }

        /// <summary>
        /// Provides a function that compares two record type arguments (one of type A and one of 
        /// object) for structural equality, this first makes sure that the `Object` argument is of 
        /// type A and then compares the *fields* from each argument for equality and returns true if 
        /// all are equal.  
        /// </summary>
        /// <remarks>You should cache the result of this method to reduce the work of building the IL 
        /// each time.  Better still use the `RecordType<A>` type to provide a cached version of these
        /// results.
        /// </remarks>
        public static Func<A, object, bool> Equals<A>()
        {
            var dynamic = new DynamicMethod("Equals", typeof(bool), new[] { typeof(A), typeof(object) }, true);
            var fields = GetPublicInstanceFields<A>(typeof(OptOutOfEqAttribute));
            var getType = GetPublicInstanceMethod<Object>("GetType").IfNone(() => throw new Exception());
            var typeEquals = GetPublicInstanceMethod<Type, Type>("Equals").IfNone(() => throw new Exception());
            var isValueType = typeof(A).GetTypeInfo().IsValueType;

            var il = dynamic.GetILGenerator();

            var argNotNullY = il.DefineLabel();
            var argNotNullX = il.DefineLabel();
            var argIsA = il.DefineLabel();
            var returnTrue = il.DefineLabel();
            var referenceUnequal = il.DefineLabel();
            var typesMatch = il.DefineLabel();

            il.DeclareLocal(typeof(A));

            // if(ReferenceEquals(x, y))
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Bne_Un_S, referenceUnequal);

            // return true
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Ret);

            il.MarkLabel(referenceUnequal);

            // if(y == null)
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Brtrue_S, argNotNullY);

            // return false
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ret);

            il.MarkLabel(argNotNullY);

            if (!isValueType)
            {
                // if(x == null)
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Brtrue_S, argNotNullX);

                // return false
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ret);

                il.MarkLabel(argNotNullX);
            }

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, getType);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Callvirt, getType);
            il.Emit(OpCodes.Callvirt, typeEquals);
            il.Emit(OpCodes.Brtrue_S, typesMatch);

            // return false
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ret);

            il.MarkLabel(typesMatch);

            il.Emit(OpCodes.Ldarg_1);
            if(isValueType)
            {
                il.Emit(OpCodes.Unbox_Any, typeof(A));
            }
            else
            {
                il.Emit(OpCodes.Castclass, typeof(A));
            }
            il.Emit(OpCodes.Stloc_0);

            foreach (var field in fields)
            {
                var continueLabel = il.DefineLabel();

                var comparerType = typeof(EqualityComparer<>).MakeGenericType(field.FieldType);
                var defaultMethod = comparerType.GetTypeInfo().DeclaredMethods.Where(m => m.Name == "get_Default").Single();
                var parms = new[] { field.FieldType, field.FieldType };
                var equalsMethod = comparerType.GetTypeInfo()
                                               .GetAllMethods()
                                               .Where(m => m.Name == "Equals")
                                               .Where(m => default(EqArray<EqDefault<Type>, Type>).Equals(
                                                               m.GetParameters().Map(p => p.ParameterType).ToArray(),
                                                               parms))
                                               .First();

                il.Emit(OpCodes.Call, defaultMethod);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(isValueType ? OpCodes.Ldflda : OpCodes.Ldfld, field);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(isValueType ? OpCodes.Ldflda : OpCodes.Ldfld, field);
                il.Emit(OpCodes.Callvirt, equalsMethod);
                il.Emit(OpCodes.Brtrue_S, continueLabel);

                // Return false
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ret);

                // Continue
                il.MarkLabel(continueLabel);
            }

            // Return true
            il.MarkLabel(returnTrue);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Ret);

            return (Func<A, object, bool>)dynamic.CreateDelegate(typeof(Func<A, object, bool>));
        }

        /// <summary>
        /// Provides a function that compares two record type arguments for structural equality, this 
        /// first compares the *fields* from each argument for equality and returns true if all are 
        /// equal.  
        /// </summary>
        /// <remarks>You should cache the result of this method to reduce the work of building the IL 
        /// each time.  Better still use the `RecordType<A>` type to provide a cached version of these
        /// results.
        /// </remarks>
        public static Func<A, A, bool> EqualsTyped<A>()
        {
            var dynamic = new DynamicMethod("EqualsTyped", typeof(bool), new[] { typeof(A), typeof(A) }, true);

            var isValueType = typeof(A).GetTypeInfo().IsValueType;
            var fields = GetPublicInstanceFields<A>(typeof(OptOutOfEqAttribute));
            var getType = GetPublicInstanceMethod<Object>("GetType").IfNone(() => throw new Exception());
            var typeEquals = GetPublicInstanceMethod<Type, Type>("Equals").IfNone(() => throw new Exception());
            var il = dynamic.GetILGenerator();
            var returnTrue = il.DefineLabel();
            var typesMatch = il.DefineLabel();
            var referenceUnequal = il.DefineLabel();

            // if(ReferenceEquals(x, y))
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Bne_Un_S, referenceUnequal);

            // return true
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Ret);

            il.MarkLabel(referenceUnequal);

            if (!isValueType)
            {
                var argNotNullX = il.DefineLabel();
                var argNotNullY = il.DefineLabel();

                // if(x == null)
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Brtrue_S, argNotNullX);

                // return false
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ret);

                il.MarkLabel(argNotNullX);

                // if(y == null)
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Brtrue_S, argNotNullY);

                // return false
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ret);

                il.MarkLabel(argNotNullY);
            }

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, getType);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Callvirt, getType);
            il.Emit(OpCodes.Callvirt, typeEquals);
            il.Emit(OpCodes.Brtrue_S, typesMatch);

            // return false
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ret);

            il.MarkLabel(typesMatch);

            foreach (var field in fields)
            {
                var continueLabel = il.DefineLabel();

                var comparerType = typeof(EqualityComparer<>).MakeGenericType(field.FieldType);
                var defaultMethod = comparerType.GetTypeInfo().DeclaredMethods.Where(m => m.Name == "get_Default").Single();
                var parms = new[] { field.FieldType, field.FieldType };
                var equalsMethod = comparerType.GetTypeInfo()
                                               .GetAllMethods()
                                               .Where(m => m.Name == "Equals")
                                               .Where(m => default(EqArray<EqDefault<Type>, Type>).Equals(
                                                               m.GetParameters().Map(p => p.ParameterType).ToArray(),
                                                               parms))
                                               .First();

                il.Emit(OpCodes.Call, defaultMethod);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(isValueType ? OpCodes.Ldflda : OpCodes.Ldfld, field);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(isValueType ? OpCodes.Ldflda : OpCodes.Ldfld, field);
                il.Emit(OpCodes.Callvirt, equalsMethod);
                il.Emit(OpCodes.Brtrue_S, continueLabel);

                // Return false
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ret);

                // Continue
                il.MarkLabel(continueLabel);
            }

            // Return true
            il.MarkLabel(returnTrue);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Ret);

            return (Func<A, A, bool>)dynamic.CreateDelegate(typeof(Func<A, A, bool>));
        }

        /// <summary>
        /// Provides a function that compares two record type arguments for structural equality, this 
        /// compares the *fields* from each argument for equality and returns 0 if all are equal, -1 
        /// if X is less than Y, and 1 if X is greater than Y.
        /// </summary>
        /// <remarks>You should cache the result of this method to reduce the work of building the IL 
        /// each time.  Better still use the `RecordType<A>` type to provide a cached version of these
        /// results.
        /// </remarks>
        public static Func<A, A, int> Compare<A>()
        {
            var dynamic = new DynamicMethod("Compare", typeof(int), new[] { typeof(A), typeof(A) }, true);

            var fields = GetPublicInstanceFields<A>(typeof(OptOutOfOrdAttribute));
            var il = dynamic.GetILGenerator();
            il.DeclareLocal(typeof(int));
            var isValueType = typeof(A).GetTypeInfo().IsValueType;
            var getType = GetPublicInstanceMethod<Object>("GetType").IfNone(() => throw new Exception());
            var typeEquals = GetPublicInstanceMethod<Type, Type>("Equals").IfNone(() => throw new Exception());
            var typesMatch = il.DefineLabel();
            var returnTrue = il.DefineLabel();
            var referenceUnequal = il.DefineLabel();

            // if(ReferenceEquals(x, y))
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Bne_Un_S, referenceUnequal);

            // return 0
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ret);

            il.MarkLabel(referenceUnequal);

            if (!isValueType)
            {
                var argNotNullX = il.DefineLabel();
                var argNotNullY = il.DefineLabel();

                // if(x == null)
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Brtrue_S, argNotNullX);

                // return 1
                il.Emit(OpCodes.Ldc_I4_M1);
                il.Emit(OpCodes.Ret);

                il.MarkLabel(argNotNullX);

                // if(y == null)
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Brtrue_S, argNotNullY);

                // return -1
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Ret);

                il.MarkLabel(argNotNullY);
            }

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, getType);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Callvirt, getType);
            il.Emit(OpCodes.Callvirt, typeEquals);
            il.Emit(OpCodes.Brtrue_S, typesMatch);

            // return false
            il.Emit(OpCodes.Ldc_I4_M1);
            il.Emit(OpCodes.Ret);

            il.MarkLabel(typesMatch);

            foreach (var field in fields)
            {
                var continueLabel = il.DefineLabel();

                var comparerType = typeof(Comparer<>).MakeGenericType(field.FieldType);
                var defaultMethod = comparerType.GetTypeInfo().DeclaredMethods.Where(m => m.Name == "get_Default").Single();
                var parms = new[] { field.FieldType, field.FieldType };
                var compareMethod = comparerType.GetTypeInfo()
                                                .GetAllMethods()
                                                .Where(m => m.Name == "Compare")
                                                .Where(m => default(EqArray<EqDefault<Type>, Type>).Equals(
                                                                m.GetParameters().Map(p => p.ParameterType).ToArray(),
                                                                parms))
                                                .First();

                il.Emit(OpCodes.Call, defaultMethod);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(isValueType ? OpCodes.Ldflda : OpCodes.Ldfld, field);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(isValueType ? OpCodes.Ldflda : OpCodes.Ldfld, field);
                il.Emit(OpCodes.Callvirt, compareMethod);
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Brfalse_S, continueLabel);

                // Return result from compare
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ret);

                // Continue
                il.MarkLabel(continueLabel);
            }

            // Return 0
            il.MarkLabel(returnTrue);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ret);

            return (Func<A, A, int>)dynamic.CreateDelegate(typeof(Func<A, A, int>));
        }

        public static Func<A, string> ToString<A>()
        {
            var isValueType = typeof(A).GetTypeInfo().IsValueType;
            var dynamic = new DynamicMethod("FieldsToString", typeof(string), new[] { typeof(A) },true);
            var fields = GetPublicInstanceFields<A>(typeof(OptOutOfToStringAttribute)).ToArray();
            var stringBuilder = GetConstructor<StringBuilder>().IfNone(() => throw new ArgumentException($"Constructor not found for StringBuilder"));
            var appendChar = GetPublicInstanceMethod<StringBuilder, char>("Append").IfNone(() => throw new ArgumentException($"Append method found for StringBuilder"));
            var appendString = GetPublicInstanceMethod<StringBuilder, string>("Append").IfNone(() => throw new ArgumentException($"Append method found for StringBuilder"));
            var toString = GetPublicInstanceMethod<Object>("ToString").IfNone(() => throw new ArgumentException($"ToString method found for Object"));
            var name = typeof(A).Name;
            if (name.IndexOf('`') != -1) name = name.Split('`').Head();

            var il = dynamic.GetILGenerator();
            il.DeclareLocal(typeof(StringBuilder));
            var notNull = il.DefineLabel();

            if (!isValueType)
            {
                // Check reference == null
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Brtrue_S, notNull);

                // Is null so return "(null)"
                il.Emit(OpCodes.Ldstr, "(null)");
                il.Emit(OpCodes.Ret);

                il.MarkLabel(notNull);
            }

            // var sb = new StringBuilder()
            il.Emit(OpCodes.Newobj, stringBuilder);
            il.Emit(OpCodes.Stloc_0);

            // sb.Append('(')
            il.Emit(OpCodes.Ldloc_0);
            if (fields.Length == 0)
            {
                il.Emit(OpCodes.Ldstr, $"{name}");
            }
            else
            {
                il.Emit(OpCodes.Ldstr, $"{name}(");
            }
            il.Emit(OpCodes.Callvirt, appendString);
            il.Emit(OpCodes.Pop);

            bool first = true;
            foreach (var field in fields)
            {
                var skipAppend = il.DefineLabel();

                if (!first)
                {
                    // sb.Append(", ")
                    il.Emit(OpCodes.Ldloc_0);
                    il.Emit(OpCodes.Ldstr, ", ");
                    il.Emit(OpCodes.Callvirt, appendString);
                    il.Emit(OpCodes.Pop);
                }

                if (!field.FieldType.GetTypeInfo().IsValueType)
                {
                    var fieldNotNull = il.DefineLabel();

                    // If(this.field == null)
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(isValueType ? OpCodes.Ldflda : OpCodes.Ldfld, field);
                    il.Emit(OpCodes.Brtrue_S, fieldNotNull);

                    // sb.Append("null")
                    il.Emit(OpCodes.Ldloc_0);
                    il.Emit(OpCodes.Ldstr, "null");
                    il.Emit(OpCodes.Callvirt, appendString);
                    il.Emit(OpCodes.Pop);

                    // continue
                    il.Emit(OpCodes.Br_S, skipAppend);
                    il.MarkLabel(fieldNotNull);
                }

                il.Emit(OpCodes.Ldloc_0);  // sb
                il.Emit(OpCodes.Ldarg_0);  // this
                il.Emit(isValueType ? OpCodes.Ldflda : OpCodes.Ldfld, field);

                var convertToString = (GetPublicStaticMethod(typeof(Convert), "ToString", field.FieldType) ||
                                       GetPublicStaticMethod(typeof(Convert), "ToString", typeof(object)))
                                      .IfNone(() => throw new Exception());

                if (field.FieldType.GetTypeInfo().IsValueType && convertToString.GetParameters().Head().ParameterType == typeof(object))
                {
                    il.Emit(OpCodes.Box, field.FieldType);
                }

                il.Emit(convertToString.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, convertToString);

                il.Emit(OpCodes.Callvirt, appendString);
                il.Emit(OpCodes.Pop);
                il.MarkLabel(skipAppend);

                first = false;
            }

            if (fields.Length > 0)
            {
                // Append(')')
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ldc_I4_S, ')');
                il.Emit(OpCodes.Callvirt, appendChar);
                il.Emit(OpCodes.Pop);
            }

            // return sb.ToString()
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Callvirt, toString);
            il.Emit(OpCodes.Ret);

            return (Func<A, string>)dynamic.CreateDelegate(typeof(Func<A, string>));
        }


        public static Action<A, SerializationInfo> GetObjectData<A>()
        {
            var isValueType = typeof(A).GetTypeInfo().IsValueType;
            var dynamic = new DynamicMethod("GetObjectData", null, new[] { typeof(A), typeof(SerializationInfo) }, true);
            var fields = GetPublicInstanceFields<A>(typeof(OptOutOfSerializationAttribute), typeof(NonSerializedAttribute));
            var argNullExcept =  GetConstructor<ArgumentNullException, string>().IfNone(() => throw new Exception());
            var il = dynamic.GetILGenerator();

            var infoIsNotNull = il.DefineLabel();

            // if(info == null)
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Brtrue_S, infoIsNotNull);

            /// throw new ArgumentNullException("info");
            il.Emit(OpCodes.Ldstr, "info");
            il.Emit(OpCodes.Newobj, argNullExcept);
            il.Emit(OpCodes.Throw);

            il.MarkLabel(infoIsNotNull);

            foreach (var field in fields)
            {
                var name = PrettyFieldName(field);

                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldstr, name);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(isValueType ? OpCodes.Ldflda : OpCodes.Ldfld, field);

                var addValue = (GetPublicInstanceMethod<SerializationInfo>("AddValue", typeof(string), field.FieldType) ||
                                GetPublicInstanceMethod<SerializationInfo>("AddValue", typeof(string), typeof(object)))
                               .IfNone(() => throw new Exception());
                if (field.FieldType.GetTypeInfo().IsValueType && addValue.GetParameters()[1].ParameterType == typeof(object))
                {
                    il.Emit(OpCodes.Box, field.FieldType);
                }

                il.Emit(OpCodes.Callvirt, addValue);
            }
            il.Emit(OpCodes.Ret);

            return (Action<A, SerializationInfo>)dynamic.CreateDelegate(typeof(Action<A, SerializationInfo>));
        }

        public static Action<A, SerializationInfo> SetObjectData<A>()
        {
            var dynamic = new DynamicMethod("SetObjectData", null, new[] { typeof(A), typeof(SerializationInfo) }, typeof(A), true);
            var fields = GetPublicInstanceFields<A>(typeof(OptOutOfSerializationAttribute), typeof(NonSerializedAttribute));
            var getTypeFromHandle = GetPublicStaticMethod<Type, RuntimeTypeHandle>("GetTypeFromHandle").IfNone(() => throw new Exception());
            var getValue = GetPublicInstanceMethod< SerializationInfo, string, Type>("GetValue").IfNone(() => throw new Exception());
            var argNullExcept = GetConstructor<ArgumentNullException, string>().IfNone(() => throw new Exception());
            var il = dynamic.GetILGenerator();

            var infoIsNotNull = il.DefineLabel();

            // if(info == null)
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Brtrue_S, infoIsNotNull);

            /// throw new ArgumentNullException("info");
            il.Emit(OpCodes.Ldstr, "info");
            il.Emit(OpCodes.Newobj, argNullExcept);
            il.Emit(OpCodes.Throw);

            il.MarkLabel(infoIsNotNull);

            foreach (var field in fields)
            {
                var name = PrettyFieldName(field);

                il.Emit(OpCodes.Ldarg_0);                   // this
                il.Emit(OpCodes.Ldarg_1);                   // info
                il.Emit(OpCodes.Ldstr, name);               // field-name
                il.Emit(OpCodes.Ldtoken, field.FieldType);  // typeof(FieldType)
                il.Emit(OpCodes.Call, getTypeFromHandle);   // Type.GetTypeFromHandle(typeof(FieldType))
                il.Emit(OpCodes.Callvirt, getValue);        // info.GetValue("field-name", FieldType)
                if (field.FieldType.GetTypeInfo().IsValueType)
                {
                    il.Emit(OpCodes.Unbox_Any, field.FieldType);
                }
                else
                {
                    il.Emit(OpCodes.Castclass, field.FieldType);
                }
                il.Emit(OpCodes.Stfld, field);
            }
            il.Emit(OpCodes.Ret);

            return (Action<A, SerializationInfo>)dynamic.CreateDelegate(typeof(Action<A, SerializationInfo>));
        }

        static string PrettyFieldName(FieldInfo field) =>
            field.Name.Split('<', '>').Match(
                ()      => "",
                x       => x,
                (x, xs) => xs.Head);
    }
}
