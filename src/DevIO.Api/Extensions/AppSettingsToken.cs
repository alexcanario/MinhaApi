namespace DevIO.Api.Extensions {
    public class AppSettingsToken {
        //Chave de criptografia
        public string Secret { get; set; }
        public int ExpiracaoHoras { get; set; }
        public string Emissor { get; set; }
        
        
        //Quais urls este token é válido
        public string ValidoEm { get; set; }
    }
}