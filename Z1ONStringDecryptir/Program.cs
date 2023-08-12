using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using dnlib.DotNet;
using dnlib.DotNet.Writer;
using System.IO;

namespace Z1ONStringDecryptor
{
    internal class Program
    {
        private static ModuleDefMD module;
        static void Main(string[] args)
        {
            module = ModuleDefMD.Load(args[0]);
            DecryptStringsReplaceMethod(module);
            var outputPath = Path.GetFileNameWithoutExtension(args[0]) + "_deobfed.exe";
            module.Write(outputPath, new ModuleWriterOptions(module)
            {
                Logger = DummyLogger.NoThrowInstance
            });
            Console.ReadKey();
        }

        public static string decryptReplace(string encryptedString, string replaceKey)
        {
            string decryptedString;
            decryptedString = encryptedString.Replace(replaceKey, null);

            return decryptedString;
        }

        static void DecryptStringsReplaceMethod(ModuleDefMD module2)
        {
            int decryptedStringCount = 0;

            foreach (var type in module.GetTypes())
            {
                foreach (var method in type.Methods)
                {
                    if (!method.HasBody)
                        continue;

                    var instructions = new List<Instruction>(method.Body.Instructions);
                    for (var i = instructions.Count - 1; i >= 0; i--)
                        if (instructions[i].OpCode.Code == Code.Ldstr &&
                            instructions[i + 1].OpCode.Code == Code.Ldstr &&
                            instructions[i + 2].OpCode.Code == Code.Ldnull &&
                            instructions[i + 3].OpCode.Code == Code.Call)
                        {
                            var String = (string)instructions[i].Operand;
                            var Key = (string)instructions[i + 1].Operand;
                            var decodedString = decryptReplace(String, Key);
                            instructions[i].Operand = decodedString;
                            instructions[i + 1].OpCode = OpCodes.Nop;
                            instructions[i + 1].Operand = null;
                            instructions[i + 2].OpCode = OpCodes.Nop;
                            instructions[i + 2].Operand = null;
                            instructions[i + 3].OpCode = OpCodes.Nop;
                            instructions[i + 3].Operand = null;

                            decryptedStringCount++;
                        }
                }
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Typewrite("# Decrypted Strings: " + decryptedStringCount + " #");
        }

        static void Typewrite(string message)
        {
            for (int i = 0; i < message.Length; i++)
            {
                Console.Write(message[i]);
                System.Threading.Thread.Sleep(1);
            }

        }
    }
}
