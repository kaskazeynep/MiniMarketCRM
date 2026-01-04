using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniMarketCRM.Application.DTO;

namespace MiniMarketCRM.Application.Interfaces
{
    public interface ICartService
    {
        Task<CartDTO> GetOrCreateAsync(int musteriId);
        Task<CartDTO> AddItemAsync(int musteriId, CartItemAddDTO dto);
        Task<CartDTO> UpdateItemAsync(int musteriId, int kalemId, CartItemUpdateDTO dto);
        Task<CartDTO> RemoveItemAsync(int musteriId, int kalemId);
        Task<SiparisDTO> CancelAsync(int musteriId);
        Task<SiparisDTO> CheckoutAsync(int musteriId);
    }
}