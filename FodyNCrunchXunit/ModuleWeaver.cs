using System;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Rocks;

public class ModuleWeaver
{
    // Will log an informational message to MSBuild
    public Action<string> LogInfo { get; set; }
    public Action<string> LogWarning { get; set; }
    public Action<string> LogDebug { get; set; }

    // An instance of Mono.Cecil.ModuleDefinition for processing
    public ModuleDefinition ModuleDefinition { get; set; }
    public IAssemblyResolver AssemblyResolver { get; set; }

    // Init logging delegates to make testing easier
    public ModuleWeaver()
    {
        LogInfo = s => { };
        LogWarning = s => { };
        LogDebug = s => { };
    }

    public void Execute()
    {
        var assemblyNameReference = ModuleDefinition.AssemblyReferences.FirstOrDefault(x => x.Name.StartsWith("NCrunch.Framework"));
        if (assemblyNameReference == null)
        {
            LogWarning("Reference to NCrunch.Framework not found.");
            return;
        }

        var ncrunchAssembly = AssemblyResolver.Resolve(assemblyNameReference);

        // new versions of NCrunch.Framework's exclusively uses attribute have an overload with a single string
        // parameter. to support older versions we'll have to use it, but I couldn't get Cecil to behave so we'll
        // just require the latest NCrunch for now I guess
        var exclusivelyUsesAttribute = ncrunchAssembly.MainModule.Types.First(t => t.Name == "ExclusivelyUsesAttribute");
        var exclusivelyUsesAttributeCtor = ModuleDefinition.ImportReference(exclusivelyUsesAttribute.Methods.First(x => x.IsConstructor && x.Parameters.Count == 1 && x.Parameters[0].ParameterType.IsArray == false));

        var potentialUnitTestTypes = ModuleDefinition.Types.Where(i => i.IsPublic && i.IsAbstract == false);
        foreach (var type in potentialUnitTestTypes)
        {
            // let's not mess with anything that already has an NCrunch namespace attribute on it
            if (type.CustomAttributes.Any(c => c.AttributeType.Namespace.StartsWith("NCrunch.Framework")))
            {
                LogDebug($"Skipping rewriting {type.Name} due to existing NCrunch attribute");
                continue;
            }

            LogDebug($"Rewriting {type.Name}");
            var usesArg = type.FullName;
            var collectionAttribute = type.CustomAttributes.FirstOrDefault(c => c.AttributeType.FullName.Equals("Xunit.CollectionAttribute"));
            if (collectionAttribute != null)
            {
                usesArg = collectionAttribute.ConstructorArguments.First().Value as string;
            }

            var exclusivelyUses = new CustomAttribute(exclusivelyUsesAttributeCtor);
            exclusivelyUses.ConstructorArguments.Add(new CustomAttributeArgument(ModuleDefinition.TypeSystem.String, usesArg));
            type.CustomAttributes.Add(exclusivelyUses);
        }
    }    
}