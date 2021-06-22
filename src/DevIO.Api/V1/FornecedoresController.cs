using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using DevIO.Api.Controllers;
using DevIO.Api.Extensions;
using DevIO.Api.ViewModels;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevIO.Api.V1 {
    [Authorize] //Para chegar aqui, precisa estar autorizado
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class FornecedoresController : MainController {
        private readonly IFornecedorRepository _fornecedorRepository;
        private readonly IFornecedorService _fornecedorService;
        private readonly IEnderecoRepository _enderecoRepository;
        private readonly IMapper _mapper;

        public FornecedoresController(  IFornecedorRepository fornecedorRepository,
                                        IEnderecoRepository enderecoRepository,
                                        IFornecedorService fornecedorService,
                                        INotificador notificador,
                                        IMapper mapper,
                                        IUser user) : base(notificador, user) {
            _fornecedorRepository = fornecedorRepository;
            _enderecoRepository = enderecoRepository;
            _fornecedorService = fornecedorService;
            _mapper = mapper;
        }

        [AllowAnonymous] //Aqui eu permito usuários anonimos, para este método
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FornecedorViewModel>>> ObterTodos() {
            var fornecedorViewModel = _mapper.Map<IEnumerable<FornecedorViewModel>>(await _fornecedorRepository.ObterTodos());
            return Ok(fornecedorViewModel);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<FornecedorViewModel>> ObterPorId(Guid id) {
            var fornecedor = await ObterFornecedoresProdutosEndereco(id);

            if (fornecedor is null) return NotFound();

            return fornecedor;
        }

        [HttpGet("obter-endereco/{id:guid}")]
        [ClaimsAuthorize("Fornecedor", "Fornecedor")]
        public async Task<EnderecoViewModel> ObterEnderecoPorId(Guid id) {
            return _mapper.Map<EnderecoViewModel>(await _enderecoRepository.ObterPorId(id));
        }

        [HttpPost]
        [ClaimsAuthorize("Fornecedor", "Adicionar")]
        public async Task<ActionResult<FornecedorViewModel>> Adicionar(FornecedorViewModel fornecedorViewModel) {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            await _fornecedorService.Adicionar(_mapper.Map<Fornecedor>(fornecedorViewModel));

            return CustomResponse(fornecedorViewModel);
        }

        [HttpPut("{id:guid}")]
        [ClaimsAuthorize("Fornecedor", "Atualizar")]
        public async Task<ActionResult<FornecedorViewModel>> Atualizar(Guid id, FornecedorViewModel fornecedorViewModel) {
            if (!id.Equals(fornecedorViewModel.Id)) {
                NotifyError("O Id informado difere do Id passado na query");
                return BadRequest(fornecedorViewModel);
            }

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            await _fornecedorService.Atualizar(_mapper.Map<Fornecedor>(fornecedorViewModel));

            return CustomResponse(fornecedorViewModel);
        }

        [HttpPut("atualizar-endereco/{id:guid}")]
        [ClaimsAuthorize("Fornecedor", "Atualizar")]
        public async Task<ActionResult<EnderecoViewModel>> AtualizarEndereco(Guid id, EnderecoViewModel enderecoViewModel) {
            if (!id.Equals(enderecoViewModel.Id)) {
                NotifyError("O Id do endereço informado difere do Id passado na query");
                return CustomResponse(enderecoViewModel);
            }

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            await _fornecedorService.AtualizarEndereco(_mapper.Map<Endereco>(enderecoViewModel));

            return CustomResponse(enderecoViewModel);
        }

        [HttpDelete("{id:guid}")]
        [ClaimsAuthorize("Fornecedor", "Remover")]
        public async Task<ActionResult<FornecedorViewModel>> Excluir(Guid id) {
            var fornecedorViewModel = await ObterFornecedorEndereco(id);

            if (fornecedorViewModel is null) return NotFound();

            var result = await _fornecedorService.Remover(id);
            if (!result) return BadRequest();
            
            return Ok(fornecedorViewModel);
        }

        private async Task<FornecedorViewModel> ObterFornecedoresProdutosEndereco(Guid id) {
            var fornecedores = _mapper.Map<FornecedorViewModel>(await _fornecedorRepository.ObterFornecedorProdutosEndereco(id));

            return fornecedores;
        }

        private async Task<FornecedorViewModel> ObterFornecedorEndereco(Guid id) {
            var fornecedorViewModel = _mapper.Map<FornecedorViewModel>(await _fornecedorRepository.ObterFornecedorEndereco(id));

            return fornecedorViewModel;
        }
    }
}