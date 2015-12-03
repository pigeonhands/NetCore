using Mono.Cecil;
using Mono.Cecil.Cil;
using NetCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetCoreBuilder.Forms
{
    public partial class mainWindow : Form
    {
        HashSet<string> ScannedTypes = new HashSet<string>();

        AssemblyDefinition newModule = null;
        AssemblyDefinition loadedModule = null;

        //CecilHelper importer = null;

        AssemblyNameReference netcoreRef;
        TypeReference objectReference = null;
        TypeReference objectReferenceNew = null;
        MethodReference CreateRemoteCallRef;
        public mainWindow()
        {
            InitializeComponent();
        }

        private void btnBuild_Click(object sender, EventArgs e)
        {
            if(!File.Exists(tbFilePath.Text) || string.IsNullOrWhiteSpace(tbOutoutAssem.Text) || string.IsNullOrWhiteSpace(tbOutoutMod.Text))
            {
                MessageBox.Show("Invalid FIle.");
                return;
            }

            loadedModule = AssemblyDefinition.ReadAssembly(tbFilePath.Text);

            foreach(var refrence in loadedModule.MainModule.AssemblyReferences)
            {
                if(refrence.FullName == "NetCore")
                {
                    netcoreRef = refrence;
                    break;
                }
            }

            AssemblyDefinition netCodeAsm = AssemblyDefinition.ReadAssembly("NetCore.dll");
            TypeDefinition netCoreType = netCodeAsm.MainModule.GetType("NetCore.NetCoreClient");

            newModule = GenerateStockType(tbOutoutAssem.Text, loadedModule);

            objectReference = loadedModule.MainModule.Import((typeof(object)));
            objectReferenceNew = newModule.MainModule.Import((typeof(object)));

            CreateRemoteCallRef = loadedModule.MainModule.Import(netCoreType.Methods.FirstOrDefault(x => x.Name == "CreateRemoteCall"));

            foreach (ModuleDefinition md in loadedModule.Modules)
            {
                //importer = new CecilImporter(newModule.MainModule);
                foreach (TypeDefinition td in md.GetTypes())
                {
                    DealWithType(td);
                }
            }

            try
            {
                newModule.Write(tbOutoutMod.Text);
                loadedModule.Write(tbOutoutAssem.Text);
                MessageBox.Show("Done.");
                
           }
           catch
           {
                MessageBox.Show("Failed.");
            }
        }

        void DealWithType(TypeDefinition type)
        {
            
            TypeAttributes att = type.Attributes;
            if (att.HasFlag(TypeAttributes.NotPublic))
                att &= ~TypeAttributes.NotPublic;

            att |= TypeAttributes.Public;

            //AutoLayout, AnsiClass, Class, Public, AutoClass, BeforeFieldInit

            //TypeDefinition nTypeDef = new TypeDefinition(type.Namespace, type.Name, att);//TypeAttributes.AutoLayout | TypeAttributes.AnsiClass | TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.BeforeFieldInit)



            //nTypeDef.IsClass = true;
            //nTypeDef.BaseType = objectReferenceNew;

            TypeDefinition nTypeDef = CecilHelper.Inject(newModule.MainModule, type);//importer.CreateImportedType(type);
            //nTypeDef.Attributes = att;
            //nTypeDef.IsClass = true;
            //nTypeDef.BaseType = objectReferenceNew;

            CustomAttribute rcAtt = null;

            bool add = false;
            foreach (MethodDefinition method in type.Methods)
            {
                
                if (!method.HasBody)
                    continue;

                bool hasAttribute = false;
                foreach(var i in method.CustomAttributes)
                {
                    if (i.AttributeType.FullName == "NetCore.RemoteCallAttribute")
                    {
                        hasAttribute = true;
                        rcAtt = i;
                        break;
                    }
                }
                if (!hasAttribute)
                    continue;

                if(!method.IsStatic)
                {
                    MessageBox.Show("RemoteCall must be only used on static methods.\n: " + method.FullName);
                    throw new Exception("Not static");
                }

                add = true;

                /*
                MethodAttributes methodAtt = method.Attributes;
                if (methodAtt.HasFlag(MethodAttributes.Private))
                    methodAtt &= ~MethodAttributes.Private;

                methodAtt |= MethodAttributes.Public;

                //MethodDefinition newMethod = new MethodDefinition(method.Name, methodAtt, method.ReturnType);
                //newMethod.Body = CecilHelper.Inject(newModule.MainModule, method);//importer.CreateImportedMethodBody(type, method, method.Body);
                //nTypeDef.Methods.Add(newMethod);
                */

                method.Body.Instructions.Clear();

                ILProcessor ilp = method.Body.GetILProcessor();
                
                ilp.Append(Instruction.Create(OpCodes.Ldstr, string.Format("{0}.{1}", type.FullName, method.Name))); //Hash this

                if (method.Parameters.Count == 0)
                {
                    ilp.Append(Instruction.Create(OpCodes.Ldc_I4_0));
                    ilp.Append(Instruction.Create(OpCodes.Newarr, objectReference));
                }
                else
                {
                    ilp.Append(Instruction.Create(OpCodes.Ldc_I4, method.Parameters.Count));
                    ilp.Append(Instruction.Create(OpCodes.Newarr, objectReference));// ths is in the old module, so we use that reference :)

                    for (int i = 0; i < method.Parameters.Count; i++)
                    {
                        ilp.Append(Instruction.Create(OpCodes.Dup));
                        ilp.Append(Instruction.Create(OpCodes.Ldc_I4, i));
                        ilp.Append(Instruction.Create(OpCodes.Ldarg, method.Parameters[i]));
                        ilp.Append(Instruction.Create(OpCodes.Stelem_Ref));
                    }
                }
                ilp.Append(Instruction.Create(OpCodes.Call, CreateRemoteCallRef));
                ilp.Append(Instruction.Create(OpCodes.Unbox_Any, method.ReturnType));
                ilp.Append(Instruction.Create(OpCodes.Ret));
            }


            if(add)
            {
                newModule.MainModule.Types.Add(nTypeDef);
            }
            

        }

        AssemblyDefinition GenerateStockType(string path, AssemblyDefinition assem)
        {
            AssemblyDefinition newModule = AssemblyDefinition.CreateAssembly(assem.Name, assem.MainModule.Name, ModuleKind.Dll);
            return newModule;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Exe File|*.exe";
                if(ofd.ShowDialog() == DialogResult.OK)
                {
                    tbFilePath.Text = ofd.FileName;
                }
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Exe File|*.exe";
                if(sfd.ShowDialog() == DialogResult.OK)
                {
                    tbOutoutAssem.Text = sfd.FileName;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "NetCore Module|*.ncm";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    tbOutoutMod.Text = sfd.FileName;
                }
            }
        }
    }
}
