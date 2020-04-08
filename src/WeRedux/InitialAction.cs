namespace WeRedux
{

    public class InitialAction : IAction, IStaticMutation
    {
        public InitialAction()
        {

        }
        public string Mutation => "@@INIT";
    }
}
