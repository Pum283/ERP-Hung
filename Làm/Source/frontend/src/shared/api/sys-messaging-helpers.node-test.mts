import test from 'node:test';
import assert from 'node:assert/strict';
import { validateMessageTemplateForm, validateChannelSendForm, renderTemplateText } from './sys-messaging-helpers.ts';

test('validateMessageTemplateForm - Empty code returns error', () => {
  const res = validateMessageTemplateForm({ code: '', channel: 'Email', subject: 'Subject', body: 'Body' });
  assert.equal(res.valid, false);
  assert.equal(res.error, 'Mã template không được để trống.');
});

test('validateMessageTemplateForm - Email missing subject returns error', () => {
  const res = validateMessageTemplateForm({ code: 'TPL_1', channel: 'Email', subject: '', body: 'Body' });
  assert.equal(res.valid, false);
  assert.equal(res.error, 'Tiêu đề email không được để trống.');
});

test('validateChannelSendForm - Invalid email format returns error', () => {
  const res = validateChannelSendForm({ channel: 'Email', target: 'invalid-email', templateCode: 'WELCOME' });
  assert.equal(res.valid, false);
  assert.equal(res.error, 'Địa chỉ Email người nhận không hợp lệ.');
});

test('validateChannelSendForm - Invalid phone format returns error', () => {
  const res = validateChannelSendForm({ channel: 'Sms', target: '123', templateCode: 'OTP' });
  assert.equal(res.valid, false);
  assert.equal(res.error, 'Số điện thoại người nhận không hợp lệ.');
});

test('validateChannelSendForm - Valid email send payload returns valid true', () => {
  const res = validateChannelSendForm({ channel: 'Email', target: 'user@domain.com', templateCode: 'WELCOME' });
  assert.equal(res.valid, true);
  assert.equal(res.error, undefined);
});

test('renderTemplateText - Replaces placeholders with values', () => {
  const rendered = renderTemplateText('Xin chào {userName}, mã đơn hàng của bạn là {orderId}.', {
    userName: 'Nguyễn Văn A',
    orderId: 'ORD-9988'
  });
  assert.equal(rendered, 'Xin chào Nguyễn Văn A, mã đơn hàng của bạn là ORD-9988.');
});
