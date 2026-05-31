using ClaimsModule.Application.Common;
using FluentAssertions;

namespace ClaimsModule.Application.Tests.Common;

public sealed class StorageBlobPathNormalizerTests
{
  [Fact]
  public void ResolveReadPath_strips_legacy_container_prefix()
  {
    var stored = "claim-documents/00000000-0000-0000-0000-000000000001/claim-id/file.pdf";
    StorageBlobPathNormalizer.ResolveReadPath(stored)
      .Should().Be("00000000-0000-0000-0000-000000000001/claim-id/file.pdf");
  }

  [Fact]
  public void ResolveReadPath_keeps_current_layout()
  {
    var stored = "00000000-0000-0000-0000-000000000001/claim-id/file.pdf";
    StorageBlobPathNormalizer.ResolveReadPath(stored).Should().Be(stored);
  }
}
