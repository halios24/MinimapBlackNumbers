using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

namespace MinimapBlackNumbers;

public class PatchColour
{
    
// I'll admit ai wrote this, I've used transpiler in the past to empty a method, but this was beyond me.
// It uses a Transpiler to modify IL code and add "label.style.color = Color.black" into the UIMinimap.UpdatePlayerBody method for both teams.
// I used a combination of dnSpy, ILSpy, and Jetbrains Rider (I believe it uses the Resharper Decompiler) to find the method and then see the IL code.
    
    [HarmonyPatch(typeof(UIMinimap), "UpdatePlayerBody")] // Replace UIMinimap with the actual class name
    public static class UIMinimap_UpdatePlayerBody_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator) // Add ILGenerator parameter
        {
            // Convert to a List so we can use CodeMatcher efficiently and modify in place
            // Also allows us to pass the modified list to the next matcher
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
    
            // --- Patch for PlayerTeam.Blue case (IL_0085 block) ---
            // Create a CodeMatcher instance for the first search
            CodeMatcher matcherBlue = new CodeMatcher(codes, generator) // Pass generator
                .MatchForward(false, // Match but don't consume
                    // IL_0085: ldloc.2
                    new CodeMatch(OpCodes.Ldloc_2), // visualElement (for Body)
                    // IL_0086: callvirt instance class [UnityEngine.UIElementsModule]UnityEngine.UIElements.IStyle [UnityEngine.UIElementsModule]UnityEngine.UIElements.VisualElement::get_style()
                    new CodeMatch(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(VisualElement), "style")),
                    // IL_008b: ldarg.0
                    new CodeMatch(OpCodes.Ldarg_0),
                    // IL_008c: ldfld valuetype [UnityEngine.CoreModule]UnityEngine.Color UIMinimap::teamBlueColor
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(UIMinimap), "teamBlueColor")),
                    // IL_0091: newobj instance void [UnityEngine.UIElementsModule]UnityEngine.UIElements.StyleColor::.ctor(valuetype [UnityEngine.CoreModule]UnityEngine.Color)
                    new CodeMatch(OpCodes.Newobj, AccessTools.Constructor(typeof(StyleColor), new[] { typeof(Color) })),
                    // IL_0096: callvirt instance void [UnityEngine.UIElementsModule]UnityEngine.UIElements.IStyle::set_unityBackgroundImageTintColor(valuetype [UnityEngine.UIElementsModule]UnityEngine.UIElements.StyleColor)
                    new CodeMatch(OpCodes.Callvirt, AccessTools.PropertySetter(typeof(IStyle), "unityBackgroundImageTintColor"))
                );
    
            // ** CORRECT WAY TO CHECK IF MATCH WAS FOUND (using ThrowIfInvalid or Pos) **
            // ThrowIfInvalid will throw an exception if the matcher is in an invalid state (i.e., match failed)
            // This is a robust way to ensure the match was successful.
            matcherBlue.ThrowIfInvalid("Harmony: Could not find PlayerTeam.Blue case start for modification in UpdatePlayerBody. Patch failed.");
    
            // The matcher is currently at the start of the matched sequence (IL_0085).
            // We want to insert *after* the set_unityBackgroundImageTintColor call (IL_0096).
            // This matched sequence is 6 instructions long.
            matcherBlue.Advance(6);
    
            // Insert new instructions for the PlayerTeam.Blue case: label.style.color = Color.black;
            // label is local variable [4] (Ldloc_S, (byte)4)
            matcherBlue.InsertAndAdvance(
                // Load label (local variable 4)
                new CodeInstruction(OpCodes.Ldloc_S, (byte)4),
                // Call label.style
                new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(VisualElement), "style")),
                // Load Color.black
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Color), "black")), // This is where the Blue team's minimap number color can be changed.
                // Create new StyleColor
                new CodeInstruction(OpCodes.Newobj, AccessTools.Constructor(typeof(StyleColor), new[] { typeof(Color) })),
                // Set label.style.color
                new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertySetter(typeof(IStyle), "color"))
            );
    
    
            // --- Patch for PlayerTeam.Red case (IL_009D block) ---
            // Create a NEW CodeMatcher instance for the second search,
            // using the *already modified* list of instructions.
            CodeMatcher matcherRed = new CodeMatcher(matcherBlue.InstructionEnumeration().ToList(), generator) // Pass modified list and generator
                   .MatchForward(false, // Match but don't consume
                    // IL_009D: ldloc.2
                    new CodeMatch(OpCodes.Ldloc_2), // visualElement (for Body)
                    // IL_009E: callvirt instance class [UnityEngine.UIElementsModule]UnityEngine.UIElements.IStyle [UnityEngine.UIElementsModule]UnityEngine.UIElements.VisualElement::get_style()
                    new CodeMatch(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(VisualElement), "style")),
                    // IL_00a3: ldarg.0
                    new CodeMatch(OpCodes.Ldarg_0),
                    // IL_00a4: ldfld valuetype [UnityEngine.CoreModule]UnityEngine.Color UIMinimap::teamRedColor
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(UIMinimap), "teamRedColor")),
                    // IL_00a9: newobj instance void [UnityEngine.UIElementsModule]UnityEngine.UIElements.StyleColor::.ctor(valuetype [UnityEngine.CoreModule]UnityEngine.Color)
                    new CodeMatch(OpCodes.Newobj, AccessTools.Constructor(typeof(StyleColor), new[] { typeof(Color) })),
                    // IL_00ae: callvirt instance void [UnityEngine.UIElementsModule]UnityEngine.UIElements.IStyle::set_unityBackgroundImageTintColor(valuetype [UnityEngine.UIElementsModule]UnityEngine.UIElements.StyleColor)
                    new CodeMatch(OpCodes.Callvirt, AccessTools.PropertySetter(typeof(IStyle), "unityBackgroundImageTintColor"))
                );
    
            // ** CORRECT WAY TO CHECK IF MATCH WAS FOUND **
            matcherRed.ThrowIfInvalid("Harmony: Could not find PlayerTeam.Red case start for modification in UpdatePlayerBody. Patch failed.");
    
            // The matcher is currently at the start of the matched sequence (IL_009D).
            // Advance past the matched 6 instructions (IL_00ae).
            matcherRed.Advance(6);
    
            // Insert new instructions for the PlayerTeam.Red case: label.style.color = Color.blue;
            // label is local variable [4]
            matcherRed.InsertAndAdvance(
                // Load label (local variable 4)
                new CodeInstruction(OpCodes.Ldloc_S, (byte)4),
                // Call label.style
                new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(VisualElement), "style")),
                // Load Color.blue
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Color), "black")), // This is where the Red team's minimap number color can be changed.
                // Create new StyleColor
                new CodeInstruction(OpCodes.Newobj, AccessTools.Constructor(typeof(StyleColor), new[] { typeof(Color) })),
                // Set label.style.color
                new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertySetter(typeof(IStyle), "color"))
            );
    
            return matcherRed.InstructionEnumeration();
        }
    }
}
    