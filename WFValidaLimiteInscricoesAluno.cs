using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Activities;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk;

namespace ProjetoDesafioII
{
    public class WFValidaLimiteInscricoesAluno : CodeActivity
    {
        #region Parametros
        [Input("Usuario")]
        [ReferenceTarget("systemuser")]
        public InArgument<EntityReference> usuarioEntrada { get; set; }

        [Input("AlunosXCursosDisponíveis")]
        [ReferenceTarget("dio_alunosxcursosdisponiveis")]
        public InArgument<EntityReference> RegistroContexto { get; set; }

        [Output("Saída")]
        public OutArgument<string> saida { get; set; }  
        #endregion
        protected override void Execute(CodeActivityContext executionContext)
        {
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            ITracingService trace = executionContext.GetExtension<ITracingService>();

            trace.Trace("dio_alunosxcursosdisponiveis: " + context.PrimaryEntityId);

            Guid guidAlunoXCurso = context.PrimaryEntityId;

            trace.Trace("guidAlunoXCurso: " + guidAlunoXCurso);

            String fetchAlunoXCursos = "<fetch distinct='false' mapping='logical' output-format='xml-platform' version='1.0'>";
            fetchAlunoXCursos += "<entity name='dio_alunosxcursosdisponiveis' >";
            fetchAlunoXCursos += "<attribute name='dio_alunosxcursosdisponiveisid' />";
            fetchAlunoXCursos += "<attribute name='dio_name' />";
            fetchAlunoXCursos += "<attribute name='dio_emcurso' />";
            fetchAlunoXCursos += "<attribute name='createdon' />";
            fetchAlunoXCursos += "<attribute name='dio_aluno' />";
            fetchAlunoXCursos += "<order descending= 'false' attribute = 'curso_name' />";
            fetchAlunoXCursos += "<filter type= 'and' >";
            fetchAlunoXCursos += "<condition attribute = 'dio_alunoxcursosdisponiveisid' value = '" + guidAlunoXCurso + "' uitype = 'dio_alunoxcursosdisponiveis'  operator= 'eq' />";
            fetchAlunoXCursos += "</filter> ";
            fetchAlunoXCursos += "</entity>";
            fetchAlunoXCursos += "</fetch> ";
            trace.Trace("fetchAlunoXCursos: " + fetchAlunoXCursos);

            var entityAlunoXCursos = service.RetrieveMultiple(new FetchExpression(fetchAlunoXCursos));
            trace.Trace("entityAlunoXCursos: " + entityAlunoXCursos.Entities.Count);

            Guid guidAluno = Guid.Empty;
            foreach (var item in entityAlunoXCursos.Entities)
            {
                string nomeCurso = item.Attributes["curso_name"].ToString();
                trace.Trace("nomeCurso: " + nomeCurso);

                var entityAluno = ((EntityReference)item.Attributes["curso_aluno"]).Id;
                guidAluno = ((EntityReference)item.Attributes["curso_aluno"]).Id;
                trace.Trace("entityAluno: " + entityAluno);
            }

            String fetchAlunoXCursosQtde = "<fetch distinct='false' mapping ='logical' output-format ='xml-platform' version = '1.0'>";
            fetchAlunoXCursosQtde += "<entity name ='dio_alunosxcursosdisponiveis'>";
            fetchAlunoXCursosQtde += "<attribute name= 'dio_alunosxcursosdisponiveisid' />";
            fetchAlunoXCursosQtde += "<attribute name= 'dio_name' />";
            fetchAlunoXCursosQtde += "<attribute name= 'dio_aluno' />";
            fetchAlunoXCursosQtde += "<attribute name= 'createdon' />";
            fetchAlunoXCursosQtde += "<order descending= 'false' attribute = 'curso_name' />";
            fetchAlunoXCursosQtde += "<filter type= 'and' >";
            fetchAlunoXCursosQtde += "<condition attribute= 'dio_aluno' value = '" + guidAluno + "' operator= 'eq' />";
            fetchAlunoXCursosQtde += "</filter>";
            fetchAlunoXCursosQtde += "</entity>";
            fetchAlunoXCursosQtde += "</fetch>";
            trace.Trace("fetchAlunoXCursosQtde: " + fetchAlunoXCursosQtde);
            var entityAlunoXCursosQtde = service.RetrieveMultiple(new FetchExpression(fetchAlunoXCursosQtde));
            trace.Trace("entityAlunoXCursosQtde: " + entityAlunoXCursosQtde.Entities.Count);
            if (entityAlunoXCursosQtde.Entities.Count > 2)
            {
                saida.Set(executionContext, "Aluno excedeu limite de cursos ativos!");
                trace.Trace("Aluno excedeu limite de cursos ativos!");
                throw new InvalidPluginExecutionException("Aluno excedeu limite de cursos ativos!");
            }
            throw new NotImplementedException();
        }
    }
}
