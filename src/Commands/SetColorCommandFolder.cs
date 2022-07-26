using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Workspace.VSIntegration.UI;

namespace SolutionColors
{
    [Export(typeof(INodeExtender))]
    public class SetColorCommandFolderProvider : INodeExtender
    {
        private readonly IWorkspaceCommandHandler _handler = new SetColorCommandFolder();

        public IChildrenSource ProvideChildren(WorkspaceVisualNodeBase parentNode) => null;

        public IWorkspaceCommandHandler ProvideCommandHandler(WorkspaceVisualNodeBase parentNode)
        {
            if (parentNode is IFolderNode)
            {
                return _handler;
            }

            return null;
        }
    }

    public class SetColorCommandFolder : IWorkspaceCommandHandler
    {
        public static bool IsRoot { get; private set; }
        
        public bool IgnoreOnMultiselect => true;

        public int Priority => 100;

        public int Exec(List<WorkspaceVisualNodeBase> selection, Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            return (int)Constants.OLECMDERR_E_NOTSUPPORTED;
        }

        public bool QueryStatus(List<WorkspaceVisualNodeBase> selection, Guid pguidCmdGroup, uint nCmdID, ref uint cmdf, ref string customTitle)
        {
            if (pguidCmdGroup == PackageGuids.SolutionColors)
            {
                IsRoot = selection[0] == selection[0].Root;
            }

            return false;
        }
    }
}
