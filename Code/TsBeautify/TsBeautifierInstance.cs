namespace TsBeautify
{
    internal class TsBeautifierInstance
    {
        public TsBeautifierInstance(string source, TsBeautifyOptions options = null)
        {
            Source = source;
            Options = options ?? new TsBeautifyOptions();
        }

        public string Source { get; }

        public TsBeautifyOptions Options { get; }

        public string Beautify()
        {
            var state = new State(new StaticState(Source, Options));
            new StateManager(state).Parse();
            return state.Output.ToString();
        }
    }
}