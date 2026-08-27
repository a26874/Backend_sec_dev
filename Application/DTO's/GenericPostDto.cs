/*
*	<copyright file="GenericPostDto">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2026 8/23/2026 4:53:12 PM</date>
*	<description></description>
**/

using System.Text.Json;

namespace Backend_sec_dev.Application.DTO_s
{
    public class GenericPostDto
    {
        public string Type { get; set; } = string.Empty;
        public JsonDocument data { get; set; } = default!;
    }
}