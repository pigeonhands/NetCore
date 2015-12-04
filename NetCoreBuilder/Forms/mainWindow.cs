using Mono.Cecil;
using Mono.Cecil.Cil;
using NetCore.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NetCoreBuilder.Forms
{
    public partial class mainWindow : Form
    {
        HashSet<string> ScannedTypes = new HashSet<string>();

        AssemblyDefinition newModule = null;
        AssemblyDefinition loadedModule = null;

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
            
            AssemblyDefinition netCodeAsm = AssemblyDefinition.ReadAssembly("NetCore.dll");
            TypeDefinition netCoreType = netCodeAsm.MainModule.GetType("NetCore.NetCoreClient");
           
            newModule = GenerateStockType(tbOutoutAssem.Text, loadedModule);

            objectReference = loadedModule.MainModule.Import((typeof(object)));
            objectReferenceNew = newModule.MainModule.Import((typeof(object)));

            CreateRemoteCallRef = loadedModule.MainModule.Import(netCoreType.Methods.FirstOrDefault(x => x.Name == "CreateRemoteCall"));

            foreach (ModuleDefinition md in loadedModule.Modules)
            {
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
           catch(Exception ex)
           {
                MessageBox.Show("Failed. \n" + ex.Message);
            }
        }

        void DealWithType(TypeDefinition type)
        {
            

            TypeAttributes att = type.Attributes;
            if (att.HasFlag(TypeAttributes.NotPublic))
                att &= ~TypeAttributes.NotPublic;

            att |= TypeAttributes.Public;

            TypeDefinition nTypeDef = CecilHelper.Inject(newModule.MainModule, type);

            if (type.CustomAttributes.Where(x => x.AttributeType.FullName == "System.CodeDom.Compiler.GeneratedCodeAttribute").Count() != 0)//type.Namespace.EndsWith(".My")
            {
                // newModule.MainModule.Types.Add(.DeclaringType);
                newModule.MainModule.Import(type);
                return;
            }

            bool add = false;
            List<MethodDefinition> RemoveMethods = new List<MethodDefinition>();


            foreach (MethodDefinition method in type.Methods)
            {


                if (!method.HasBody)
                    continue;
                if (!method.IsStatic)
                    continue;

                TransportAction visibility = TransportAction.Public;
               
                if (!TypeCheck.KeepMethod(type, method, out visibility))
                    continue;

                if(!method.IsStatic)
                {
                    MessageBox.Show("RemoteCall must be only used on static methods.\n: " + method.FullName);
                    throw new Exception("Not static");
                }

                add = true;

                if(visibility == TransportAction.Move)
                {
                    RemoveMethods.Add(method);
                    continue;
                }

                if (visibility == TransportAction.Copy)
                    continue;

                method.Body.Instructions.Clear();

                ILProcessor ilp = method.Body.GetILProcessor();

                if (visibility == TransportAction.MoveClear)
                {
                    ilp.Append(Instruction.Create(OpCodes.Ret));
                    continue;
                }

               
                
                ilp.Append(Instruction.Create(OpCodes.Ldstr, Hashing.SHA(string.Format("{0}.{1}", type.FullName, method.Name))));

                if (method.Parameters.Count == 0)
                {
                    ilp.Append(Instruction.Create(OpCodes.Ldc_I4_0));
                    ilp.Append(Instruction.Create(OpCodes.Newarr, objectReference));
                }
                else
                {
                    ilp.Append(Instruction.Create(OpCodes.Ldc_I4, method.Parameters.Count));
                    ilp.Append(Instruction.Create(OpCodes.Newarr, objectReference));

                    for (int i = 0; i < method.Parameters.Count; i++)
                    {
                        ilp.Append(Instruction.Create(OpCodes.Dup));
                        ilp.Append(Instruction.Create(OpCodes.Ldc_I4, i));
                        ilp.Append(Instruction.Create(OpCodes.Ldarg, method.Parameters[i]));
                        ilp.Append(Instruction.Create(OpCodes.Box, method.Parameters[i].ParameterType));
                        ilp.Append(Instruction.Create(OpCodes.Stelem_Ref));
                    }
                }
                ilp.Append(Instruction.Create(OpCodes.Call, CreateRemoteCallRef));
                ilp.Append(Instruction.Create(OpCodes.Unbox_Any, method.ReturnType));
                ilp.Append(Instruction.Create(OpCodes.Ret));
            }

            foreach (MethodDefinition md in RemoveMethods)
                type.Methods.Remove(md);

            if (add)
            {
                newModule.MainModule.Types.Add(nTypeDef);
            }
        }

        AssemblyDefinition GenerateStockType(string path, AssemblyDefinition assem)
        {
            AssemblyDefinition newModule = AssemblyDefinition.CreateAssembly(assem.Name, assem.MainModule.Name, ModuleKind.Dll);
           // foreach (var asm in assem.MainModule.AssemblyReferences)
             //   newModule.MainModule.AssemblyReferences.Add(asm);
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
