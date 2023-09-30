using HarmonyLib;
using FrooxEngine;
using FrooxEngine.UIX;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ResoniteModLoader;

namespace RenameContextMenuArcs
{
    public class RenameContextMenuArcs : ResoniteMod
    {
        public override string Name => "RenameContextMenuArcs";
        public override string Author => "TheJebForge";
        public override string Version => "1.1.0";

        public override void OnEngineInit() {
            Harmony harmony = new Harmony($"net.{Author}.{Name}");
            harmony.PatchAll();
        }

        public static void SetName(UIBuilder ui, ArcData data) {
            ValueCopy<string> val = ui.Current.AttachComponent<ValueCopy<string>>();
            val.Source.Target = data.text.Content;
            val.Target.Target = ui.Current.NameField;
        }

        [HarmonyPatch(typeof(UIBuilder), nameof(UIBuilder.Arc))]
        class UIBuilder_Arc_Patch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                int startIndex = -1;

                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

                for (int i = 0; i < codes.Count; i++) {
                    CodeInstruction instr = codes[i];
                    if (instr.opcode != OpCodes.Call || !((MethodInfo)instr.operand).Name.Contains("NestOut")) continue;
                    Msg("Found!");
                    startIndex = i + 1;
                    break;
                }

                if (startIndex > -1) {
                    MethodInfo method = typeof(RenameContextMenuArcs).GetMethod("SetName");
                    
                    codes.InsertRange(startIndex,
                        new[]
                        {
                            new CodeInstruction(OpCodes.Ldarg_0),
                            new CodeInstruction(OpCodes.Ldloc_0),
                            new CodeInstruction(OpCodes.Call, method)
                        });
                    Msg("Patched");
                }

                return codes.AsEnumerable();
            }
        }
    }
}