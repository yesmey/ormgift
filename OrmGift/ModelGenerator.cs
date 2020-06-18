using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using OrmGift.CodeAnalysis;
using OrmGift.DataModeling;
using OrmGift.Generators;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OrmGift
{
    [Generator]
    public class ModelGenerator : ISourceGenerator
    {
        public void Initialize(InitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new ModelSyntaxReceiver());
        }

        public void Execute(SourceGeneratorContext context)
        {
            if (!(context.SyntaxReceiver is ModelSyntaxReceiver syntaxReceiver))
                return;

            var compilation = context.Compilation;

            foreach (var syntax in syntaxReceiver.Types)
            {
                var semanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);
                var type = semanticModel.GetDeclaredSymbol(syntax);
                if (type.HasAttribute(typeof(DataModelAttribute).FullName))
                {
                    var modelType = new ModelType((INamedTypeSymbol)type);
                    var typeCode = DbTypeGenerator.Generate(modelType);
                    var contextCode = DbTypeContextGenerator.Generate(modelType);

                    context.AddSource($"{type.Name}.Generated.cs", SourceText.From(typeCode, Encoding.UTF8));
                    context.AddSource($"{type.Name}DataContext.Generated.cs", SourceText.From(contextCode, Encoding.UTF8));
                }
            }
        }
    }

    internal class ModelSyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> Types = new List<ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax && classDeclarationSyntax.AttributeLists.Count > 0)
            {
                Types.Add(classDeclarationSyntax);
            }
        }
    }
}
