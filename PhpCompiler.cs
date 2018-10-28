using System.Collections.Generic;
using System.Linq;
using Nebula.SDK.Compiler.Abstracts;
using Nebula.SDK.Plugin;
using Nebula.SDK.Objects;

namespace Nebula.SDK.Compiler.Objects.PHP
{
    public class PhpCompiler : AbstractCompiler
    {
        public PhpCompiler()
        {
            
        }

        public override List<OutputFile> Compile()
        {
            var outputFiles = new List<OutputFile>();
            var projectName = Project.GetProperName();
            
            var entityNamespace = new AbstractNamespace { 
                Name = projectName + @"\" + TemplateMeta.EntityLocation
            };

            var clientNamespace = new AbstractNamespace {
                Name = projectName + @"\" + TemplateMeta.ClientLocation,
                Imports = new List<string> {
                    
                    @"JsonMapper",
                    $"{projectName}\\{TemplateMeta.EntityLocation}"
                }
            };

            var entityClasses = GetClassesByType<EntityNode, PhpEntityClass>(entityNamespace, this);
            var clientClasses = GetClassesByType<ApiNode, PhpClientClass>(clientNamespace, this);

            outputFiles.AddRange(entityClasses.Select(c => new PhpOutputFile(c, TemplateMeta)));
            outputFiles.AddRange(clientClasses.Select(c => new PhpOutputFile(c, TemplateMeta)));
            return outputFiles;
        }
    }
}