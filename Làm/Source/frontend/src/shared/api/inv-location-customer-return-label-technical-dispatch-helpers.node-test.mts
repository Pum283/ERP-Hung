import test from 'node:test';
import assert from 'node:assert/strict';
import {
  formatBinLocationCode,
  getInspectionConditionLabel,
} from './inv-location-customer-return-label-technical-dispatch-helpers.ts';

test('formatBinLocationCode - formats zone, aisle, rack and bin into standard location code', () => {
  assert.equal(formatBinLocationCode('ZONE-A', 'Aisle-01', 'Rack-03', 'Bin-05'), 'ZONE-A-Aisle-01-Rack-03-Bin-05');
  assert.equal(formatBinLocationCode('', '', '', ''), 'ZONE-A-A1-R1-B1');
});

test('getInspectionConditionLabel - maps customer return condition to label and status styling', () => {
  const good = getInspectionConditionLabel('GoodRestockable');
  assert.equal(good.label, 'Hàng Đạt Chuẩn - Nhập Lại Kho');
  assert.match(good.colorClass, /bg-emerald/);

  const scrap = getInspectionConditionLabel('DamagedScrap');
  assert.equal(scrap.label, 'Hàng Hư Hỏng - Thanh Lý / Phế Liệu');
  assert.match(scrap.colorClass, /bg-rose/);
});
