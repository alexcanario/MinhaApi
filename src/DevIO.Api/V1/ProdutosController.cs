using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using DevIO.Api.Controllers;
using DevIO.Api.Extensions;
using DevIO.Api.ViewModels;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DevIO.Api.V1 {
    [ApiVersion("1.0")]
    [Route("api/produtos")]
    [Route("api/v{version:apiVersion}/produtos")]
    [Authorize]
    public class ProdutosController : MainController {
        private readonly IProdutoRepository _produtoRepository;
        private readonly IProdutoService _produtoService;
        private readonly IMapper _mapper;
        public ProdutosController(  IProdutoRepository produtoRepository,
                                    IProdutoService produtoService,
                                    INotificador notificador,
                                    IMapper mapper,
                                    IUser user) : base(notificador, user) {
            _produtoRepository = produtoRepository;
            _produtoService = produtoService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IEnumerable<ProdutoViewModel>> ObterTodos() {
            var produtos = await _produtoRepository.ObterProdutosFornecedores();
            var produtosViewModel = _mapper.Map<IEnumerable<ProdutoViewModel>>(produtos);
            return produtosViewModel;
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> ObterPorId(Guid id) {
            var produtoViewModel = await ObterProduto(id);
            if (produtoViewModel is null) return NotFound();

            return CustomResponse(produtoViewModel);
        }

        [HttpPost]
        [ClaimsAuthorize("Produto", "Adicionar")]
        public async Task<ActionResult<ProdutoViewModel>> Adicionar(ProdutoViewModel produtoViewModel) {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var imagemNome = $"{Guid.NewGuid()}_{produtoViewModel.Imagem}";
            if(!UploadArquivo(produtoViewModel.ImagemUpload, imagemNome)) {
                return CustomResponse(produtoViewModel);
            }

            await _produtoService.Adicionar(_mapper.Map<Produto>(produtoViewModel));

            return CustomResponse(produtoViewModel);
        }

        [HttpPost("adicionar")]
        [ClaimsAuthorize("Produto", "Adicionar")]
        public async Task<ActionResult<ProdutoViewModel>> AdicionarAlternativo(ProdutoImagemViewModel produtoViewModel) {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var imgPrefixo = $"{Guid.NewGuid()}_";
            if (!await UploadArquivoAlternativo(produtoViewModel.ImagemUpload, imgPrefixo)) {
                return CustomResponse(produtoViewModel);
            }

            produtoViewModel.Imagem = imgPrefixo + produtoViewModel.ImagemUpload.FileName;
            await _produtoService.Adicionar(_mapper.Map<Produto>(produtoViewModel));

            return CustomResponse(produtoViewModel);
        }

        [RequestSizeLimit(40000000)]
        //[DisableRequestSizeLimit]
        [HttpPost("imagem")]
        public async Task<ActionResult> AdicionarImagem(IFormFile file) {
            return Ok(file);
        }

        [ClaimsAuthorize("Produto", "Atualizar")]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ProdutoImagemViewModel>> Atualizar(Guid id, ProdutoViewModel produtoViewModel) {
            if (!id.Equals(produtoViewModel.Id)) {
                NotifyError("O Id informado é diferente do Id encontrado!");
                return CustomResponse();
            }

            var produtoAtualizacao = await ObterProduto(id);

            produtoViewModel.Imagem = produtoAtualizacao.Imagem;

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            if (!produtoViewModel.ImagemUpload.Equals(null)) {
                var nomeImagem = $"{Guid.NewGuid()}_{produtoViewModel.Imagem}";
                if (!UploadArquivo(produtoViewModel.ImagemUpload, nomeImagem)) {
                    return CustomResponse(ModelState);
                }

                produtoAtualizacao.Imagem = nomeImagem;
            }

            produtoAtualizacao.Nome = produtoViewModel.Nome;
            produtoAtualizacao.Descricao = produtoViewModel.Descricao;
            produtoAtualizacao.Valor = produtoViewModel.Valor;
            produtoAtualizacao.Ativo = produtoViewModel.Ativo;

            await _produtoService.Atualizar(_mapper.Map<Produto>(produtoAtualizacao));

            return CustomResponse(produtoViewModel);
        }

        private async Task<ProdutoViewModel> ObterProduto(Guid id) {
            return _mapper.Map<ProdutoViewModel>(await _produtoRepository.ObterProdutoFornecedor(id));
        }

        private bool UploadArquivo(string arquivo, string nomeArquivo) {
            if(string.IsNullOrEmpty(arquivo)) {
                NotifyError("Forneça uma imagem para este produto");
                return false;
            }

            var caminhoArquivo = Path.Combine(Directory.GetCurrentDirectory(),  "wwwroot/app/demo-webapi/src/assets", nomeArquivo);
            if(System.IO.File.Exists(caminhoArquivo)) {
                NotifyError("Já existe um arquivo com este nome, escolha outro!");
                return false;
            }

            var imagemBinario = Convert.FromBase64String(arquivo);
            System.IO.File.WriteAllBytes(caminhoArquivo, imagemBinario);
            return true;
        }

        private async Task<bool> UploadArquivoAlternativo(IFormFile arquivo, string imgPrefixo) {
            if (arquivo is null || arquivo.Length.Equals(0)) {
                NotifyError("Forneça uma imagem para este produto!");
                return false;
            }

            var caminhoArquivo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/app/demo-webapi/src/assets/", imgPrefixo + arquivo.FileName);
            if (System.IO.File.Exists(caminhoArquivo)) {
                NotifyError("Já existe um arquivo com mesmo nome, escolha outro nome");
                return false;
            }

            using (var stream = new FileStream(caminhoArquivo, FileMode.Create)) {
                await arquivo.CopyToAsync(stream);
            }

            return true;
        }

    }
}