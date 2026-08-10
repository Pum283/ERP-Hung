import test from 'node:test';
import assert from 'node:assert/strict';
import {
  validateEnqueueOutbox,
  getOutboxStatusBadgeColor,
  validateEmailGatewayForm,
  validateSmsGatewayForm,
  validateChatAttachment,
  validateSendChatMessage,
} from './sys-outbox-gateways-helpers.ts';

// ─── UC_SYS_087: validateEnqueueOutbox ───

test('validateEnqueueOutbox - valid payload returns true', () => {
  const res = validateEnqueueOutbox({ eventType: 'SYS.EVENT', sourceModule: 'SYS', payloadJson: '{"id":1}' });
  assert.equal(res.valid, true);
});

test('validateEnqueueOutbox - empty eventType returns error', () => {
  const res = validateEnqueueOutbox({ eventType: '', sourceModule: 'SYS', payloadJson: '{}' });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('EventType'));
});

test('validateEnqueueOutbox - invalid JSON returns error', () => {
  const res = validateEnqueueOutbox({ eventType: 'EVT', sourceModule: 'SYS', payloadJson: 'NOT_JSON' });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('JSON'));
});

test('getOutboxStatusBadgeColor - returns correct hex codes', () => {
  assert.equal(getOutboxStatusBadgeColor('Published'), '#10b981');
  assert.equal(getOutboxStatusBadgeColor('Failed'), '#ef4444');
  assert.equal(getOutboxStatusBadgeColor('Pending'), '#f59e0b');
});

// ─── UC_SYS_088: validateEmailGatewayForm ───

test('validateEmailGatewayForm - valid SMTP config', () => {
  const res = validateEmailGatewayForm('GW_SMTP', 'Gmail', {
    providerType: 'Smtp',
    smtpHost: 'smtp.gmail.com',
    smtpPort: 587,
    useSsl: true,
    senderEmail: 'admin@system.com',
    senderName: 'Admin',
  });
  assert.equal(res.valid, true);
});

test('validateEmailGatewayForm - invalid providerType returns error', () => {
  const res = validateEmailGatewayForm('GW_1', 'Name', {
    providerType: 'Invalid' as any,
    smtpHost: 'host',
    smtpPort: 25,
    useSsl: false,
    senderEmail: 'test@test.com',
    senderName: 'Test',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('Smtp, SendGrid, AmazonSES'));
});

test('validateEmailGatewayForm - missing SmtpHost returns error', () => {
  const res = validateEmailGatewayForm('GW_1', 'Name', {
    providerType: 'Smtp',
    smtpHost: '',
    smtpPort: 587,
    useSsl: true,
    senderEmail: 'test@test.com',
    senderName: 'Test',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('SmtpHost'));
});

// ─── UC_SYS_089: validateSmsGatewayForm ───

test('validateSmsGatewayForm - valid VietGuys config', () => {
  const res = validateSmsGatewayForm('GW_SMS', 'VietGuys', {
    providerType: 'VietGuys',
    senderId: 'BRAND_XYZ',
    accountSidOrUser: 'user1',
    apiKeyOrSecret: 'secret1',
  });
  assert.equal(res.valid, true);
});

test('validateSmsGatewayForm - missing SenderId returns error', () => {
  const res = validateSmsGatewayForm('GW_SMS', 'VietGuys', {
    providerType: 'VietGuys',
    senderId: '',
    accountSidOrUser: 'user1',
    apiKeyOrSecret: 'secret1',
  });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('SenderId'));
});

// ─── UC_SYS_101: validateChatAttachment & validateSendChatMessage ───

test('validateChatAttachment - allowed PDF file returns valid', () => {
  const res = validateChatAttachment('document.pdf', 5 * 1024 * 1024);
  assert.equal(res.valid, true);
});

test('validateChatAttachment - forbidden .exe extension returns error', () => {
  const res = validateChatAttachment('malware.exe', 1024);
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('bảo mật'));
});

test('validateChatAttachment - file size > 25MB returns error', () => {
  const res = validateChatAttachment('video.mp4', 30 * 1024 * 1024);
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('25MB'));
});

test('validateSendChatMessage - text only returns valid', () => {
  const res = validateSendChatMessage({ conversationId: 'c1', body: 'Hello world' });
  assert.equal(res.valid, true);
});

test('validateSendChatMessage - attachment only returns valid', () => {
  const res = validateSendChatMessage({ conversationId: 'c1', body: '', attachmentFileId: 'f1' });
  assert.equal(res.valid, true);
});

test('validateSendChatMessage - empty text and no attachment returns error', () => {
  const res = validateSendChatMessage({ conversationId: 'c1', body: '   ' });
  assert.equal(res.valid, false);
  assert.ok(res.error?.includes('văn bản hoặc file'));
});
