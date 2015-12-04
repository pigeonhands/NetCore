
using Mono.Cecil;
using Mono.Cecil.Cil;
using NetCoreBuilder;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;



public static class CecilHelper
{
    private static TypeDefinition _Inject(ModuleDefinition mod, TypeDefinition type, Dictionary<MetadataToken, IMemberDefinition> mems)
    {
        TypeAttributes att = type.Attributes;
        if (att.HasFlag(TypeAttributes.NotPublic))
            att &= ~TypeAttributes.NotPublic;

        att |= TypeAttributes.Public;
        TypeDefinition definition = new TypeDefinition(type.Namespace, type.Name, att)
        {
            ClassSize = type.ClassSize,
            PackingSize = type.PackingSize
        };
        if (type.BaseType != null)
        {
            definition.BaseType = mod.Import(type.BaseType);
        }
        mems.Add(type.MetadataToken, definition);
        foreach (TypeDefinition definition2 in type.NestedTypes)
        {
            TypeDefinition item = _Inject(mod, definition2, mems);
            definition.NestedTypes.Add(item);
        }
        foreach (FieldDefinition definition4 in type.Fields)
        {
            if (!definition4.IsLiteral)
            {
                FieldDefinition definition5 = new FieldDefinition(definition4.Name, definition4.Attributes, mod.TypeSystem.Void);
                mems.Add(definition4.MetadataToken, definition5);
                definition.Fields.Add(definition5);
            }
        }
        foreach (MethodDefinition definition6 in type.Methods)
        {
            Visibility visible = Visibility.Public;
            if (!TypeCheck.KeepMethod(type, definition6, out visible))
                continue;

            MethodAttributes methodAtt = definition6.Attributes;
            if (methodAtt.HasFlag(MethodAttributes.Private))
                methodAtt &= ~MethodAttributes.Private;

            if (visible == Visibility.Public)
                methodAtt |= MethodAttributes.Public;
            else
                methodAtt |= MethodAttributes.Private;

            MethodDefinition definition7 = new MethodDefinition(definition6.Name, methodAtt, definition6.ReturnType);
            mems.Add(definition6.MetadataToken, definition7);
            definition.Methods.Add(definition7);
        }
        return definition;
    }

    public static string GetName(TypeDefinition typeDef)
    {
        if (typeDef.DeclaringType == null)
        {
            return typeDef.Name;
        }
        StringBuilder builder = new StringBuilder();
        builder.Append(typeDef.Name);
        while (typeDef.DeclaringType != null)
        {
            typeDef = typeDef.DeclaringType;
            builder.Insert(0, typeDef.Name + "/");
        }
        return builder.ToString();
    }

    public static string GetNamespace(TypeDefinition typeDef)
    {
        while (typeDef.DeclaringType != null)
        {
            typeDef = typeDef.DeclaringType;
        }
        return typeDef.Namespace;
    }

    public static string GetVersionName(this AssemblyDefinition asm)
    {
        return (asm.Name.Name + ", Version=" + asm.Name.Version);
    }

    public static string GetVersionName(this AssemblyNameReference asmName)
    {
        return (asmName.Name + ", Version=" + asmName.Version);
    }

    private static FieldReference ImportField(FieldReference fldRef, ModuleDefinition mod, Dictionary<MetadataToken, IMemberDefinition> mems)
    {
        if ((mems != null) && mems.ContainsKey(fldRef.MetadataToken))
        {
            return (mems[fldRef.MetadataToken] as FieldReference);
        }
        if (fldRef.DeclaringType.Scope.Name != "Confuser.Core.Injections.dll")
        {
            return mod.Import(fldRef);
        }
        return fldRef;
    }

    private static MethodReference ImportMethod(MethodReference mtdRef, ModuleDefinition mod, MethodReference context, Dictionary<MetadataToken, IMemberDefinition> mems)
    {
        MethodReference reference = mtdRef;
        if (mtdRef is GenericInstanceMethod)
        {
            GenericInstanceMethod method = mtdRef as GenericInstanceMethod;
            reference = new GenericInstanceMethod(ImportMethod(method.ElementMethod, mod, context, mems));
            foreach (TypeReference reference2 in method.GenericArguments)
            {
                (reference as GenericInstanceMethod).GenericArguments.Add(ImportType(reference2, mod, context, mems));
            }
            reference.ReturnType = ImportType(reference.ReturnType, mod, reference, mems);
            foreach (ParameterDefinition definition in reference.Parameters)
            {
                definition.ParameterType = ImportType(definition.ParameterType, mod, reference, mems);
            }
        }
        else if ((mems != null) && mems.ContainsKey(mtdRef.MetadataToken))
        {
            reference = mems[mtdRef.MetadataToken] as MethodReference;
        }
        else if (mtdRef.DeclaringType.Scope.Name != "Confuser.Core.Injections.dll")
        {
            reference = mod.Import(reference);
            reference.ReturnType = ImportType(reference.ReturnType, mod, reference, mems);
            foreach (ParameterDefinition definition2 in reference.Parameters)
            {
                definition2.ParameterType = ImportType(definition2.ParameterType, mod, reference, mems);
            }
        }
        if (!(mtdRef is MethodDefinition) && !(mtdRef is MethodSpecification))
        {
            reference.DeclaringType = ImportType(mtdRef.DeclaringType, mod, context, mems);
        }
        return reference;
    }

    private static TypeReference ImportType(TypeReference typeRef, ModuleDefinition mod, MethodReference context, Dictionary<MetadataToken, IMemberDefinition> mems)
    {
        TypeReference reference = typeRef;
        if (typeRef is TypeSpecification)
        {
            if (typeRef is ArrayType)
            {
                ArrayType type = typeRef as ArrayType;
                reference = new ArrayType(ImportType(type.ElementType, mod, context, mems));
                (reference as ArrayType).Dimensions.Clear();
                foreach (ArrayDimension dimension in type.Dimensions)
                {
                    (reference as ArrayType).Dimensions.Add(dimension);
                }
                return reference;
            }
            if (typeRef is GenericInstanceType)
            {
                GenericInstanceType type2 = typeRef as GenericInstanceType;
                reference = new GenericInstanceType(ImportType(type2.ElementType, mod, context, mems));
                foreach (TypeReference reference2 in type2.GenericArguments)
                {
                    (reference as GenericInstanceType).GenericArguments.Add(ImportType(reference2, mod, context, mems));
                }
                return reference;
            }
            if (typeRef is OptionalModifierType)
            {
                return new OptionalModifierType(ImportType((typeRef as OptionalModifierType).ModifierType, mod, context, mems), ImportType((typeRef as TypeSpecification).ElementType, mod, context, mems));
            }
            if (typeRef is RequiredModifierType)
            {
                return new RequiredModifierType(ImportType((typeRef as RequiredModifierType).ModifierType, mod, context, mems), ImportType((typeRef as TypeSpecification).ElementType, mod, context, mems));
            }
            if (typeRef is ByReferenceType)
            {
                return new ByReferenceType(ImportType((typeRef as TypeSpecification).ElementType, mod, context, mems));
            }
            if (typeRef is PointerType)
            {
                return new PointerType(ImportType((typeRef as TypeSpecification).ElementType, mod, context, mems));
            }
            if (typeRef is PinnedType)
            {
                return new PinnedType(ImportType((typeRef as TypeSpecification).ElementType, mod, context, mems));
            }
            if (!(typeRef is SentinelType))
            {
                throw new NotSupportedException();
            }
            return new SentinelType(ImportType((typeRef as TypeSpecification).ElementType, mod, context, mems));
        }
        if (typeRef is GenericParameter)
        {
            if (((context != null) && !((typeRef as GenericParameter).Owner is TypeReference)) && ((typeRef as GenericParameter).Position < context.GenericParameters.Count))
            {
                return context.GenericParameters[(typeRef as GenericParameter).Position];
            }
            return typeRef;
        }
        if ((mems != null) && mems.ContainsKey(typeRef.MetadataToken))
        {
            return (mems[typeRef.MetadataToken] as TypeReference);
        }
        if (!(reference is TypeDefinition) && (typeRef.Scope.Name != "Confuser.Core.Injections.dll"))
        {
            reference = mod.Import(reference);
        }
        return reference;
    }

    public static MethodDefinition Inject(ModuleDefinition mod, MethodDefinition mtd)
    {
        MethodDefinition context = new MethodDefinition(mtd.Name, mtd.Attributes, mod.TypeSystem.Void)
        {
            Attributes = mtd.Attributes,
            ImplAttributes = mtd.ImplAttributes
        };
        if (mtd.IsPInvokeImpl)
        {
            context.PInvokeInfo = mtd.PInvokeInfo;
            bool flag = false;
            foreach (ModuleReference reference in mod.ModuleReferences)
            {
                if (reference.Name == context.PInvokeInfo.Module.Name)
                {
                    flag = true;
                    context.PInvokeInfo.Module = reference;
                    break;
                }
            }
            if (!flag)
            {
                mod.ModuleReferences.Add(context.PInvokeInfo.Module);
            }
        }
        if (mtd.HasCustomAttributes)
        {
            foreach (CustomAttribute attribute in mtd.CustomAttributes)
            {
                CustomAttribute item = new CustomAttribute(ImportMethod(attribute.Constructor, mod, context, null), attribute.GetBlob());
                context.CustomAttributes.Add(item);
            }
        }
        foreach (GenericParameter parameter in mtd.GenericParameters)
        {
            GenericParameter parameter2 = new GenericParameter(parameter.Name, context);
            if (parameter.HasCustomAttributes)
            {
                foreach (CustomAttribute attribute3 in parameter.CustomAttributes)
                {
                    CustomAttribute attribute4 = new CustomAttribute(ImportMethod(attribute3.Constructor, mod, context, null), attribute3.GetBlob());
                    parameter2.CustomAttributes.Add(attribute4);
                }
            }
            context.GenericParameters.Add(parameter2);
        }
        context.ReturnType = ImportType(mtd.ReturnType, mod, context, null);
        foreach (ParameterDefinition definition2 in mtd.Parameters)
        {
            ParameterDefinition definition3 = new ParameterDefinition(definition2.Name, definition2.Attributes, ImportType(definition2.ParameterType, mod, context, null));
            if (definition2.HasCustomAttributes)
            {
                foreach (CustomAttribute attribute5 in definition2.CustomAttributes)
                {
                    CustomAttribute attribute6 = new CustomAttribute(ImportMethod(attribute5.Constructor, mod, context, null), attribute5.GetBlob());
                    definition3.CustomAttributes.Add(attribute6);
                }
            }
            context.Parameters.Add(definition3);
        }
        if (mtd.HasBody)
        {
            MethodBody body = mtd.Body;
            MethodBody body2 = new MethodBody(context)
            {
                MaxStackSize = body.MaxStackSize,
                InitLocals = body.InitLocals
            };
            ILProcessor iLProcessor = body2.GetILProcessor();
            foreach (VariableDefinition definition4 in body.Variables)
            {
                body2.Variables.Add(new VariableDefinition(definition4.Name, ImportType(definition4.VariableType, mod, context, null)));
            }
            foreach (Instruction instruction in body.Instructions)
            {
                switch (instruction.OpCode.OperandType)
                {
                    case OperandType.InlineTok:
                        {
                            if (!(instruction.Operand is TypeReference))
                            {
                                goto Label_052B;
                            }
                            iLProcessor.Emit(instruction.OpCode, ImportType(instruction.Operand as TypeReference, mod, context, null));
                            continue;
                        }
                    case OperandType.InlineType:
                        {
                            iLProcessor.Emit(instruction.OpCode, ImportType(instruction.Operand as TypeReference, mod, context, null));
                            continue;
                        }
                    case OperandType.InlineVar:
                    case OperandType.ShortInlineVar:
                        {
                            int num2 = body.Variables.IndexOf(instruction.Operand as VariableDefinition);
                            iLProcessor.Emit(instruction.OpCode, body2.Variables[num2]);
                            continue;
                        }
                    case OperandType.InlineArg:
                    case OperandType.ShortInlineArg:
                        {
                            if (instruction.Operand != body.ThisParameter)
                            {
                                break;
                            }
                            iLProcessor.Emit(instruction.OpCode, body2.ThisParameter);
                            continue;
                        }
                    case OperandType.InlineMethod:
                        {
                            if (instruction.Operand == mtd)
                            {
                                iLProcessor.Emit(instruction.OpCode, context);
                            }
                            else
                            {
                                iLProcessor.Emit(instruction.OpCode, ImportMethod(instruction.Operand as MethodReference, mod, context, null));
                            }
                            continue;
                        }
                    case OperandType.InlineField:
                        {
                            iLProcessor.Emit(instruction.OpCode, ImportField(instruction.Operand as FieldReference, mod, null));
                            continue;
                        }
                    default:
                        goto Label_058E;
                }
                int index = mtd.Parameters.IndexOf(instruction.Operand as ParameterDefinition);
                iLProcessor.Emit(instruction.OpCode, context.Parameters[index]);
                continue;
                Label_052B:
                if (instruction.Operand is FieldReference)
                {
                    iLProcessor.Emit(instruction.OpCode, ImportField(instruction.Operand as FieldReference, mod, null));
                }
                else if (instruction.Operand is MethodReference)
                {
                    iLProcessor.Emit(instruction.OpCode, ImportMethod(instruction.Operand as MethodReference, mod, context, null));
                }
                continue;
                Label_058E:
                iLProcessor.Append(instruction);
            }
            for (int i = 0; i < body2.Instructions.Count; i++)
            {
                Instruction instruction2 = body2.Instructions[i];
                Instruction instruction3 = body.Instructions[i];
                if (instruction2.OpCode.OperandType == OperandType.InlineSwitch)
                {
                    Instruction[] operand = (Instruction[])instruction3.Operand;
                    Instruction[] instructionArray2 = new Instruction[operand.Length];
                    for (int j = 0; j < instructionArray2.Length; j++)
                    {
                        instructionArray2[j] = body2.Instructions[body.Instructions.IndexOf(operand[j])];
                    }
                    instruction2.Operand = instructionArray2;
                }
                else if ((instruction2.OpCode.OperandType == OperandType.ShortInlineBrTarget) || (instruction2.OpCode.OperandType == OperandType.InlineBrTarget))
                {
                    instruction2.Operand = body2.Instructions[body.Instructions.IndexOf(instruction2.Operand as Instruction)];
                }
            }
            foreach (ExceptionHandler handler in body.ExceptionHandlers)
            {
                ExceptionHandler handler2 = new ExceptionHandler(handler.HandlerType);
                if (body.Instructions.IndexOf(handler.TryStart) != -1)
                {
                    handler2.TryStart = body2.Instructions[body.Instructions.IndexOf(handler.TryStart)];
                }
                if (body.Instructions.IndexOf(handler.TryEnd) != -1)
                {
                    handler2.TryEnd = body2.Instructions[body.Instructions.IndexOf(handler.TryEnd)];
                }
                if (body.Instructions.IndexOf(handler.HandlerStart) != -1)
                {
                    handler2.HandlerStart = body2.Instructions[body.Instructions.IndexOf(handler.HandlerStart)];
                }
                if (body.Instructions.IndexOf(handler.HandlerEnd) != -1)
                {
                    handler2.HandlerEnd = body2.Instructions[body.Instructions.IndexOf(handler.HandlerEnd)];
                }
                switch (handler.HandlerType)
                {
                    case ExceptionHandlerType.Catch:
                        handler2.CatchType = ImportType(handler.CatchType, mod, context, null);
                        break;

                    case ExceptionHandlerType.Filter:
                        handler2.FilterStart = body2.Instructions[body.Instructions.IndexOf(handler.FilterStart)];
                        break;
                }
                body2.ExceptionHandlers.Add(handler2);
            }
            context.Body = body2;
        }
        return context;
    }

    public static TypeDefinition Inject(ModuleDefinition mod, TypeDefinition type)
    {
        //type.Module.FullLoad();
        Dictionary<MetadataToken, IMemberDefinition> mems = new Dictionary<MetadataToken, IMemberDefinition>();
        TypeDefinition definition = _Inject(mod, type, mems);
        PopulateDatas(mod, type, mems);
        return definition;
    }

    private static void PopulateDatas(ModuleDefinition mod, TypeDefinition type, Dictionary<MetadataToken, IMemberDefinition> mems)
    {
        TypeDefinition definition = mems[type.MetadataToken] as TypeDefinition;
        if (type.BaseType != null)
        {
            definition.BaseType = ImportType(type.BaseType, mod, null, mems);
        }
        foreach (TypeDefinition definition2 in type.NestedTypes)
        {
            PopulateDatas(mod, definition2, mems);
        }
        foreach (FieldDefinition definition3 in type.Fields)
        {
            if (!definition3.IsLiteral)
            {
                (mems[definition3.MetadataToken] as FieldDefinition).FieldType = ImportType(definition3.FieldType, mod, null, mems);
            }
        }
        foreach (MethodDefinition definition4 in type.Methods)
        {
            Visibility visible = Visibility.Public;
            if (!TypeCheck.KeepMethod(type, definition4, out visible))
                continue;

            MethodAttributes methodAtt = definition4.Attributes;
            if (methodAtt.HasFlag(MethodAttributes.Private))
                methodAtt &= ~MethodAttributes.Private;

            if(visible == Visibility.Public)
                methodAtt |= MethodAttributes.Public;
            else
                methodAtt |= MethodAttributes.Private;

            definition4.Attributes = methodAtt;

            PopulateMethod(mod, definition4, mems[definition4.MetadataToken] as MethodDefinition, mems);
        }
    }

    public static void PopulateMethod(ModuleDefinition mod, MethodDefinition mtd, MethodDefinition newMtd, Dictionary<MetadataToken, IMemberDefinition> mems)
    {
        //mtd.Module.FullLoad();
        newMtd.Attributes = mtd.Attributes;
        newMtd.ImplAttributes = mtd.ImplAttributes;
        if (mtd.IsPInvokeImpl)
        {
            newMtd.PInvokeInfo = mtd.PInvokeInfo;
            bool flag = false;
            foreach (ModuleReference reference in mod.ModuleReferences)
            {
                if (reference.Name == newMtd.PInvokeInfo.Module.Name)
                {
                    flag = true;
                    newMtd.PInvokeInfo.Module = reference;
                    break;
                }
            }
            if (!flag)
            {
                mod.ModuleReferences.Add(newMtd.PInvokeInfo.Module);
            }
        }
        if (mtd.HasCustomAttributes)
        {
            foreach (CustomAttribute attribute in mtd.CustomAttributes)
            {
                CustomAttribute item = new CustomAttribute(ImportMethod(attribute.Constructor, mod, newMtd, mems), attribute.GetBlob());
                newMtd.CustomAttributes.Add(item);
            }
        }
        foreach (GenericParameter parameter in mtd.GenericParameters)
        {
            GenericParameter parameter2 = new GenericParameter(parameter.Name, newMtd);
            if (parameter.HasCustomAttributes)
            {
                foreach (CustomAttribute attribute3 in parameter.CustomAttributes)
                {
                    CustomAttribute attribute4 = new CustomAttribute(mod.Import(attribute3.Constructor), attribute3.GetBlob());
                    parameter2.CustomAttributes.Add(attribute4);
                }
            }
            newMtd.GenericParameters.Add(parameter2);
        }
        newMtd.ReturnType = ImportType(mtd.ReturnType, mod, newMtd, mems);
        foreach (ParameterDefinition definition in mtd.Parameters)
        {
            ParameterDefinition definition2 = new ParameterDefinition(definition.Name, definition.Attributes, ImportType(definition.ParameterType, mod, newMtd, mems));
            if (definition.HasCustomAttributes)
            {
                foreach (CustomAttribute attribute5 in definition.CustomAttributes)
                {
                    CustomAttribute attribute6 = new CustomAttribute(ImportMethod(attribute5.Constructor, mod, newMtd, mems), attribute5.GetBlob());
                    definition2.CustomAttributes.Add(attribute6);
                }
            }
            newMtd.Parameters.Add(definition2);
        }
        if (mtd.HasBody)
        {
            MethodBody body = mtd.Body;
            MethodBody body2 = new MethodBody(newMtd)
            {
                MaxStackSize = body.MaxStackSize,
                InitLocals = body.InitLocals
            };
            ILProcessor iLProcessor = body2.GetILProcessor();
            foreach (VariableDefinition definition3 in body.Variables)
            {
                body2.Variables.Add(new VariableDefinition(definition3.Name, ImportType(definition3.VariableType, mod, newMtd, mems)));
            }
            foreach (Instruction instruction in body.Instructions)
            {
                switch (instruction.OpCode.OperandType)
                {
                    case OperandType.InlineTok:
                        {
                            if (!(instruction.Operand is FieldReference))
                            {
                                goto Label_04F6;
                            }
                            iLProcessor.Emit(instruction.OpCode, ImportField(instruction.Operand as FieldReference, mod, mems));
                            continue;
                        }
                    case OperandType.InlineType:
                        {
                            iLProcessor.Emit(instruction.OpCode, ImportType(instruction.Operand as TypeReference, mod, newMtd, mems));
                            continue;
                        }
                    case OperandType.InlineVar:
                    case OperandType.ShortInlineVar:
                        {
                            int num2 = body.Variables.IndexOf(instruction.Operand as VariableDefinition);
                            iLProcessor.Emit(instruction.OpCode, body2.Variables[num2]);
                            continue;
                        }
                    case OperandType.InlineArg:
                    case OperandType.ShortInlineArg:
                        {
                            if (instruction.Operand != body.ThisParameter)
                            {
                                break;
                            }
                            iLProcessor.Emit(instruction.OpCode, body2.ThisParameter);
                            continue;
                        }
                    case OperandType.InlineMethod:
                        {
                            iLProcessor.Emit(instruction.OpCode, ImportMethod(instruction.Operand as MethodReference, mod, newMtd, mems));
                            continue;
                        }
                    case OperandType.InlineField:
                        {
                            iLProcessor.Emit(instruction.OpCode, ImportField(instruction.Operand as FieldReference, mod, mems));
                            continue;
                        }
                    default:
                        goto Label_055A;
                }
                int index = mtd.Parameters.IndexOf(instruction.Operand as ParameterDefinition);
                iLProcessor.Emit(instruction.OpCode, newMtd.Parameters[index]);
                continue;
                Label_04F6:
                if (instruction.Operand is MethodReference)
                {
                    iLProcessor.Emit(instruction.OpCode, ImportMethod(instruction.Operand as MethodReference, mod, newMtd, mems));
                }
                else if (instruction.Operand is TypeReference)
                {
                    iLProcessor.Emit(instruction.OpCode, ImportType(instruction.Operand as TypeReference, mod, newMtd, mems));
                }
                continue;
                Label_055A:
                iLProcessor.Append(instruction);
            }
            for (int i = 0; i < body2.Instructions.Count; i++)
            {
                Instruction instruction2 = body2.Instructions[i];
                Instruction instruction3 = body.Instructions[i];
                if (instruction2.OpCode.OperandType == OperandType.InlineSwitch)
                {
                    Instruction[] operand = (Instruction[])instruction3.Operand;
                    Instruction[] instructionArray2 = new Instruction[operand.Length];
                    for (int j = 0; j < instructionArray2.Length; j++)
                    {
                        instructionArray2[j] = body2.Instructions[body.Instructions.IndexOf(operand[j])];
                    }
                    instruction2.Operand = instructionArray2;
                }
                else if ((instruction2.OpCode.OperandType == OperandType.ShortInlineBrTarget) || (instruction2.OpCode.OperandType == OperandType.InlineBrTarget))
                {
                    instruction2.Operand = body2.Instructions[body.Instructions.IndexOf(instruction2.Operand as Instruction)];
                }
            }
            foreach (ExceptionHandler handler in body.ExceptionHandlers)
            {
                ExceptionHandler handler2 = new ExceptionHandler(handler.HandlerType);
                if (body.Instructions.IndexOf(handler.TryStart) != -1)
                {
                    handler2.TryStart = body2.Instructions[body.Instructions.IndexOf(handler.TryStart)];
                }
                if (body.Instructions.IndexOf(handler.TryEnd) != -1)
                {
                    handler2.TryEnd = body2.Instructions[body.Instructions.IndexOf(handler.TryEnd)];
                }
                if (body.Instructions.IndexOf(handler.HandlerStart) != -1)
                {
                    handler2.HandlerStart = body2.Instructions[body.Instructions.IndexOf(handler.HandlerStart)];
                }
                if (body.Instructions.IndexOf(handler.HandlerEnd) != -1)
                {
                    handler2.HandlerEnd = body2.Instructions[body.Instructions.IndexOf(handler.HandlerEnd)];
                }
                switch (handler.HandlerType)
                {
                    case ExceptionHandlerType.Catch:
                        handler2.CatchType = ImportType(handler.CatchType, mod, newMtd, mems);
                        break;

                    case ExceptionHandlerType.Filter:
                        handler2.FilterStart = body2.Instructions[body.Instructions.IndexOf(handler.FilterStart)];
                        break;
                }
                body2.ExceptionHandlers.Add(handler2);
            }
            newMtd.Body = body2;
        }
    }

    public static void RefreshTokens(ModuleDefinition mod)
    {
        int type = 1;
        int fld = 1;
        int mtd = 1;
        foreach (TypeDefinition definition in mod.Types)
        {
            RefreshType(ref type, ref fld, ref mtd, definition);
        }
    }

    private static void RefreshType(ref int type, ref int fld, ref int mtd, TypeDefinition typeDef)
    {
        typeDef.MetadataToken = new MetadataToken(Mono.Cecil.TokenType.TypeDef, type);
        type++;
        foreach (FieldDefinition definition in typeDef.Fields)
        {
            definition.MetadataToken = new MetadataToken(Mono.Cecil.TokenType.Field, fld);
            fld++;
        }
        foreach (MethodDefinition definition2 in typeDef.Methods)
        {
            definition2.MetadataToken = new MetadataToken(Mono.Cecil.TokenType.Method, mtd);
            mtd++;
        }
        foreach (TypeDefinition definition3 in typeDef.NestedTypes)
        {
            RefreshType(ref type, ref fld, ref mtd, definition3);
        }
    }

    public static void Replace(MethodBody body, Instruction inst, Instruction[] news)
    {
        int index = body.Instructions.IndexOf(inst);
        if (index == -1)
        {
            throw new InvalidOperationException();
        }
        body.Instructions.RemoveAt(index);
        for (int i = news.Length - 1; i >= 0; i--)
        {
            body.Instructions.Insert(index, news[i]);
        }
        foreach (Instruction instruction in body.Instructions)
        {
            if ((instruction.Operand is Instruction) && (instruction.Operand == inst))
            {
                instruction.Operand = news[0];
            }
            else if (instruction.Operand is Instruction[])
            {
                Instruction[] operand = instruction.Operand as Instruction[];
                int num3 = Array.IndexOf<Instruction>(operand, inst);
                if (num3 != -1)
                {
                    operand[num3] = news[0];
                }
            }
        }
        foreach (ExceptionHandler handler in body.ExceptionHandlers)
        {
            if (handler.TryStart == inst)
            {
                handler.TryStart = news[0];
            }
            if (handler.TryEnd == inst)
            {
                handler.TryEnd = news[0];
            }
            if (handler.HandlerStart == inst)
            {
                handler.HandlerStart = news[0];
            }
            if (handler.HandlerEnd == inst)
            {
                handler.HandlerEnd = news[0];
            }
            if (handler.FilterStart == inst)
            {
                handler.FilterStart = news[0];
            }
        }
    }
}