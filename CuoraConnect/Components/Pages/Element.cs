using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MudBlazor.Examples.Data.Models
{
    public class Element
    {
        public int Number
        {
            get; set;
        }
        public string Sign
        {
            get; set;
        }
        public string Name
        {
            get; set;
        }
        public int Position
        {
            get; set;
        }
        public double Molar
        {
            get; set;
        }
        public string Network_Name
        {
            get; set;
        }
        public string Default_Gateway
        {
            get; set;
        }
        public string IP_Address
        {
            get; set;
        }
        public string ImageUrl
        {
            get; set;
        } // Nova propriedade para a URL da imagem
    }

}