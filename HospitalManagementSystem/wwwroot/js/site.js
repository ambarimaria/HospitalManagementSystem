// Auto-dismiss alerts
document.querySelectorAll('.alert').forEach(el => {
  setTimeout(() => el.style.opacity = '0', 4000);
  setTimeout(() => el.remove(), 4300);
  el.style.transition = 'opacity 0.3s';
});

// Confirm delete
document.querySelectorAll('[data-confirm]').forEach(el => {
  el.addEventListener('click', e => {
    if (!confirm(el.dataset.confirm || 'Are you sure?')) e.preventDefault();
  });
});

// Dynamic bill items
function addBillItem() {
  const idx = document.querySelectorAll('.bill-item-row').length;
  const html = `<tr class="bill-item-row">
    <td><input name="Items[${idx}].Description" class="form-control" placeholder="Service description" required /></td>
    <td><select name="Items[${idx}].Category" class="form-control form-select">
      <option>Consultation</option><option>Procedure</option><option>Medicine</option><option>Radiology</option><option>Lab</option><option>Room</option><option>Nursing</option><option>General</option>
    </select></td>
    <td><input name="Items[${idx}].Quantity" type="number" min="1" value="1" class="form-control item-qty" oninput="calcRow(this)" /></td>
    <td><input name="Items[${idx}].UnitPrice" type="number" step="0.01" min="0" class="form-control item-price" oninput="calcRow(this)" /></td>
    <td class="item-total text-right font-semibold">$0.00</td>
    <td><button type="button" class="btn btn-ghost btn-sm text-danger" onclick="this.closest('tr').remove(); updateBillTotal()"><i class="fas fa-trash"></i></button></td>
  </tr>`;
  document.getElementById('bill-items-body').insertAdjacentHTML('beforeend', html);
}

function calcRow(el) {
  const row = el.closest('tr');
  const qty = parseFloat(row.querySelector('.item-qty')?.value) || 0;
  const price = parseFloat(row.querySelector('.item-price')?.value) || 0;
  const totalEl = row.querySelector('.item-total');
  if (totalEl) totalEl.textContent = '$' + (qty * price).toFixed(2);
  updateBillTotal();
}

function updateBillTotal() {
  let sub = 0;
  document.querySelectorAll('.bill-item-row').forEach(row => {
    const qty = parseFloat(row.querySelector('.item-qty')?.value) || 0;
    const price = parseFloat(row.querySelector('.item-price')?.value) || 0;
    sub += qty * price;
  });
  const disc = parseFloat(document.getElementById('discount-input')?.value) || 0;
  const tax = parseFloat(document.getElementById('tax-input')?.value) || 0;
  const taxAmt = (sub - disc) * tax / 100;
  const total = sub - disc + taxAmt;
  const subEl = document.getElementById('bill-subtotal');
  const taxEl = document.getElementById('bill-tax');
  const totalEl = document.getElementById('bill-total');
  if (subEl) subEl.textContent = '$' + sub.toFixed(2);
  if (taxEl) taxEl.textContent = '$' + taxAmt.toFixed(2);
  if (totalEl) totalEl.textContent = '$' + total.toFixed(2);
}

// Initialize bill row calculations on page load
document.querySelectorAll('.bill-item-row').forEach(row => {
  const qty = row.querySelector('.item-qty');
  if (qty) calcRow(qty);
});
